using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Ribosoft.Data;
using Ribosoft.Models;
using Ribosoft.Services;
using Ribosoft.Blast;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Ribosoft.Jobs
{
    public class GenerateCandidates
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IEmailSender _emailSender;
        private readonly RibosoftAlgo _ribosoftAlgo;
        private readonly MultiObjectiveOptimization.MultiObjectiveOptimizer _multiObjectiveOptimizer;
        private readonly IConfiguration _configuration;
        private readonly Blaster _blaster;

        private ApplicationDbContext _db;

        public GenerateCandidates(DbContextOptions<ApplicationDbContext> options, IEmailSender emailSender, IConfiguration configuration)
        {
            _dbOptions = options;
            _db =  new ApplicationDbContext(options);
            _emailSender = emailSender;
            _ribosoftAlgo = new RibosoftAlgo();
            _multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();
            _configuration = configuration;
            _blaster = new Blaster();
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task Phase1(int jobId, IJobCancellationToken cancellationToken)
        {
            var job = GetJob(jobId);

            // TODO - temporarily catch retried jobs
            await DoStage(job, JobState.Errored, j => j.JobState != JobState.New, async (j, c) => {}, cancellationToken);

            // run candidate generator
            await DoStage(job, JobState.CandidateGenerator, j => j.JobState == JobState.New, RunCandidateGenerator, cancellationToken);
            
            // queue phase 2 job for in-vivo runs (blast)
            await DoStage(job, JobState.QueuedPhase2, j => j.JobState == JobState.CandidateGenerator && j.TargetEnvironment == TargetEnvironment.InVivo, async (j, c) =>
                {
                    BackgroundJob.Enqueue<GenerateCandidates>(x => x.Phase2(j.Id, c));
                }, cancellationToken);
            
            // queue phase 3 job for in-vitro runs, skipping phase 2 (MOO)
            await DoStage(job, JobState.QueuedPhase3, j => j.JobState == JobState.CandidateGenerator && j.TargetEnvironment == TargetEnvironment.InVitro, async (j, c) =>
            {
                BackgroundJob.Enqueue<GenerateCandidates>(x => x.Phase3(j.Id, c));
            }, cancellationToken);
        }

        [Queue("blast")]
        [AutomaticRetry(Attempts = 0)]
        public async Task Phase2(int jobId, IJobCancellationToken cancellationToken)
        {
            var job = GetJob(jobId);
            
            // TODO - temporarily catch retried jobs
            await DoStage(job, JobState.Errored, j => j.JobState != JobState.QueuedPhase2, async (j, c) => {}, cancellationToken);

            // run blast to calculate specificity
            await DoStage(job, JobState.Specificity, j => j.JobState == JobState.QueuedPhase2, RunBlast, cancellationToken);
            
            // queue phase 3 job (MOO)
            await DoStage(job, JobState.QueuedPhase3, j => j.JobState == JobState.Specificity, async (j, c) =>
            {
                BackgroundJob.Enqueue<GenerateCandidates>(x => x.Phase3(j.Id, c));
            }, cancellationToken);
        }
        
        [AutomaticRetry(Attempts = 0)]
        public async Task Phase3(int jobId, IJobCancellationToken cancellationToken)
        {
            var job = GetJob(jobId);
            
            // TODO - temporarily catch retried jobs
            await DoStage(job, JobState.Errored, j => j.JobState != JobState.QueuedPhase3, async (j, c) => {}, cancellationToken);

            // run multi-objective optimization
            await DoStage(job, JobState.MultiObjectiveOptimization, j => j.JobState == JobState.QueuedPhase3, MultiObjectiveOptimize, cancellationToken);
            
            // complete job
            await DoStage(job, JobState.Completed, j => j.JobState == JobState.MultiObjectiveOptimization, CompleteJob, cancellationToken); 
        }

        private async Task DoStage(Job job, JobState state, Func<Job, bool> acceptFunc, Func<Job, IJobCancellationToken, Task> func, IJobCancellationToken cancellationToken)
        {
            if (!acceptFunc(job))
            {
                // don't do anything if the stage can't handle this type of job
                // this also catches errored jobs, etc.
                return;
            }
            
            cancellationToken.ThrowIfCancellationRequested();
            
            // set the job to this stage's state
            if (job.JobState != state)
            {
                job.JobState = state;
                await _db.SaveChangesAsync();
            }

            await func(job, cancellationToken);
        }

        private async Task RecreateDbContext()
        {
            await _db.SaveChangesAsync();
            _db = new ApplicationDbContext(_dbOptions);
        }

        private Job GetJob(int jobId)
        {
            return _db.Jobs
                .Include(j => j.Owner)
                .Include(j => j.Assembly)
                .Include(j => j.Ribozyme)
                    .ThenInclude(r => r.RibozymeStructures)
                .Single(j => j.Id == jobId);
        }

        private async Task RunCandidateGenerator(Job job, IJobCancellationToken cancellationToken)
        {
            var idealStructurePattern = new Regex(@"[^.^(^)]");

            List<string> rnaInputs = new List<string>();

            if (job.FivePrime && job.OpenReadingFrame && job.ThreePrime)
            {
                rnaInputs.Add(job.RNAInput);
            }
            else if (job.FivePrime && job.OpenReadingFrame)
            {
                rnaInputs.Add(job.RNAInput.Substring(0, job.OpenReadingFrameEnd));
            }
            else if (job.OpenReadingFrame && job.ThreePrime)
            {
                rnaInputs.Add(job.RNAInput.Substring(job.OpenReadingFrameStart, job.RNAInput.Length - job.OpenReadingFrameStart - 1));
            }
            else if (job.FivePrime && job.ThreePrime)
            {
                rnaInputs.Add(job.RNAInput.Substring(0, job.OpenReadingFrameStart));
                rnaInputs.Add(job.RNAInput.Substring(job.OpenReadingFrameEnd, job.RNAInput.Length - job.OpenReadingFrameEnd - 1));
            }
            else if (job.FivePrime)
            {
                rnaInputs.Add(job.RNAInput.Substring(0, job.OpenReadingFrameStart));
            }
            else if (job.OpenReadingFrame)
            {
                rnaInputs.Add(job.RNAInput.Substring(job.OpenReadingFrameStart, job.OpenReadingFrameEnd - job.OpenReadingFrameStart - 1));
            }
            else if (job.ThreePrime)
            {
                rnaInputs.Add(job.RNAInput.Substring(job.OpenReadingFrameEnd, job.RNAInput.Length - job.OpenReadingFrameEnd - 1));
            }
            else
            {
                job.JobState = JobState.Errored;
                job.StatusMessage = "No Target Region Selected!";
                await _db.SaveChangesAsync();
                return;
            }

            foreach (var rnaInput in rnaInputs)
            {
                CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

                foreach (var ribozymeStructure in job.Ribozyme.RibozymeStructures)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    IEnumerable<Candidate> candidates;

                    try
                    {
                        // Candidate Generation
                        candidates = candidateGenerator.GenerateCandidates(
                            ribozymeStructure.Sequence,
                            ribozymeStructure.Structure,
                            ribozymeStructure.SubstrateTemplate,
                            ribozymeStructure.SubstrateStructure,
                            rnaInput);
                    }
                    catch (CandidateGeneration.CandidateGenerationException e)
                    {
                        job.JobState = JobState.Errored;
                        job.StatusMessage = e.Message;
                        await _db.SaveChangesAsync();
                        return;
                    }

                    // Algorithms
                    try
                    {
                        uint batchCount = 0;
                        _db.ChangeTracker.AutoDetectChangesEnabled = false;

                        foreach (var candidate in candidates)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            string ideal = idealStructurePattern.Replace(candidate.Structure, ".");

                            var accessibilityScore = _ribosoftAlgo.Accessibility(candidate, job.RNAInput,
                                ribozymeStructure.Cutsite + candidate.CutsiteNumberOffset);
                            var structureScore = _ribosoftAlgo.Structure(candidate, ideal);
                            var temperatureScore = _ribosoftAlgo.Anneal(candidate, candidate.SubstrateSequence,
                                candidate.SubstrateStructure, job.Na.GetValueOrDefault(), job.Probe.GetValueOrDefault());

                            _db.Designs.Add(new Design
                            {
                                JobId = job.Id,

                                Sequence = candidate.Sequence.GetString(),
                                CutsiteIndex = candidate.CutsiteIndices.First(),
                                SubstrateSequenceLength = candidate.SubstrateSequence.Length,

                                AccessibilityScore = accessibilityScore,
                                StructureScore = structureScore,
                                HighestTemperatureScore = -1.0f * temperatureScore,
                                DesiredTemperatureScore = Math.Abs(temperatureScore - job.Temperature.GetValueOrDefault())
                            });

                            if (++batchCount % 100 == 0)
                            {
                                await _db.SaveChangesAsync();
                                await RecreateDbContext();
                                _db.ChangeTracker.AutoDetectChangesEnabled = false;
                                batchCount = 0;
                            }
                        }

                        await RecreateDbContext();
                    }
                    catch (RibosoftAlgoException e)
                    {
                        job.JobState = JobState.Errored;
                        job.StatusMessage = e.Code.ToString();
                        return;
                    }
                    finally
                    {
                        _db.ChangeTracker.AutoDetectChangesEnabled = true;
                        _db.Jobs.Attach(job);
                        await _db.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task MultiObjectiveOptimize(Job job, IJobCancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _multiObjectiveOptimizer.Optimize(_db.Designs.Where(j => j.JobId == job.Id).ToList(), 1);
            }
            catch (MultiObjectiveOptimization.MultiObjectiveOptimizationException e)
            {
                job.JobState = JobState.Errored;
                job.StatusMessage = e.Message;
            }
            finally
            {
                await _db.SaveChangesAsync();
            }
        }

        private async Task RunBlast(Job job, IJobCancellationToken cancellationToken)
        {
            // check if blastn is available; if it isn't, ignore specificity
            if (!_blaster.IsAvailable())
            {
                return;
            }
            
            var designs = _db.Designs
                             .Where(d => d.JobId == job.Id)
                             .GroupBy(d => new { d.CutsiteIndex, d.SubstrateSequenceLength });

            foreach (var designGroup in designs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var design = designGroup.First();
                var substrateSequence = job.RNAInput.Substring(design.CutsiteIndex, design.SubstrateSequenceLength);

                if (string.IsNullOrEmpty(substrateSequence))
                {
                    continue;
                }
                
                var sb = new StringBuilder();
                sb.AppendFormat(">{0}{1}", design.Id, Environment.NewLine);
                sb.AppendFormat("{0}{1}", substrateSequence, Environment.NewLine);
                var blastParameters = BlastParametersForQuery(job.Assembly.Path, substrateSequence);
                blastParameters.Query = sb.ToString();
                var output = _blaster.Run(blastParameters);

                var specificityScore = 0.0f;

                foreach (var line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var fields = line.Split('\t');

                    // filter out X_ predictions
                    //if (fields[1].StartsWith('X'))
                    //{
                    //    continue;
                    //}

                    specificityScore += float.Parse(fields[2]) * float.Parse(fields[3]) / 10000;
                }

                designGroup.All(x => { x.SpecificityScore = specificityScore; return true; });
            }

            await _db.SaveChangesAsync();
        }

        private BlastParameters BlastParametersForQuery(string database, string query)
        {
            var blastParameters = new BlastParameters
            {
                BlastDbPath = _configuration.GetValue("Blast:BLASTDB", string.Empty),
                Database = database,
                UseIndex = true,
                LowercaseMasking = true,
                OutputFormat = "6 qseqid saccver pident qcovs",
                NumThreads = _configuration.GetValue("Blast:NumThreads", 4),
                MaxTargetSequences = 200
            };

            if (query.Length <= 30)
            {
                // adjust parameters for a short query (as NCBI does it)
                blastParameters.Task = BlastParameters.BlastTask.blastn_short;
                blastParameters.ExpectValue = 1000.0f;
            }

            return blastParameters;
        }

        private async Task CompleteJob(Job job, IJobCancellationToken cancellationToken)
        {
            await SendJobCompletionEmail(job.Owner);
        }

        private async Task SendJobCompletionEmail(ApplicationUser user)
        {
            await _emailSender.SendEmailAsync(user.Email, "Job completed", "Your Ribosoft job has completed.");
        }
    }
}
