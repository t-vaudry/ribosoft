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
        public async Task Generate(int jobId, IJobCancellationToken cancellationToken)
        {
            var job = GetJob(jobId);

            if (job.JobState != JobState.New)
            {
                return;
            }

            // check if blastn is available; if it isn't, ignore specificity
            bool shouldCalculateSpecificity = _blaster.IsAvailable();

            job.JobState = JobState.Started;
            await _db.SaveChangesAsync();

            await RunCandidateGenerator(job, cancellationToken);

            if (shouldCalculateSpecificity)
            {
                await RunBlast(job, cancellationToken);
            }

            await MultiObjectiveOptimize(job, cancellationToken);

            job.JobState = JobState.Completed;
            await _db.SaveChangesAsync();

            await SendJobCompletionEmail(job.Owner);
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
                .Include(j => j.Ribozyme)
                    .ThenInclude(r => r.RibozymeStructures)
                .Single(j => j.Id == jobId);
        }

        private async Task RunCandidateGenerator(Job job, IJobCancellationToken cancellationToken)
        {
            var idealStructurePattern = new Regex(@"[^.^(^)]");

            List<string> RNAInputs = new List<string>();

            if (job.FivePrime && job.OpenReadingFrame && job.ThreePrime)
            {
                RNAInputs.Add(job.RNAInput);
            }
            else if (job.FivePrime && job.OpenReadingFrame)
            {
                RNAInputs.Add(job.RNAInput.Substring(0, job.OpenReadingFrameEnd));
            }
            else if (job.OpenReadingFrame && job.ThreePrime)
            {
                RNAInputs.Add(job.RNAInput.Substring(job.OpenReadingFrameStart, job.RNAInput.Length - job.OpenReadingFrameStart - 1));
            }
            else if (job.FivePrime && job.ThreePrime)
            {
                RNAInputs.Add(job.RNAInput.Substring(0, job.OpenReadingFrameStart));
                RNAInputs.Add(job.RNAInput.Substring(job.OpenReadingFrameEnd, job.RNAInput.Length - job.OpenReadingFrameEnd - 1));
            }
            else if (job.FivePrime)
            {
                RNAInputs.Add(job.RNAInput.Substring(0, job.OpenReadingFrameStart));
            }
            else if (job.OpenReadingFrame)
            {
                RNAInputs.Add(job.RNAInput.Substring(job.OpenReadingFrameStart, job.OpenReadingFrameEnd - job.OpenReadingFrameStart - 1));
            }
            else if (job.ThreePrime)
            {
                RNAInputs.Add(job.RNAInput.Substring(job.OpenReadingFrameEnd, job.RNAInput.Length - job.OpenReadingFrameEnd - 1));
            }
            else
            {
                job.JobState = JobState.Errored;
                job.StatusMessage = "No Target Region Selected!";
                await _db.SaveChangesAsync();
                return;
            }

            foreach (var rnaInput in RNAInputs)
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
                            job.RNAInput);
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
            var designs = _db.Designs
                             .Where(d => d.JobId == job.Id)
                             .GroupBy(d => new { d.CutsiteIndex, d.SubstrateSequenceLength });

            foreach (var designGroup in designs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var design = designGroup.First();
                var substrateSequence = job.RNAInput.Substring(design.CutsiteIndex, design.SubstrateSequenceLength);
                var sb = new StringBuilder();
                sb.AppendFormat(">{0}{1}", design.Id, Environment.NewLine);
                sb.AppendFormat("{0}{1}", substrateSequence, Environment.NewLine);
                var blastParameters = BlastParametersForQuery(substrateSequence);
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

        private BlastParameters BlastParametersForQuery(string query)
        {
            var regularBlastParameters = new BlastParameters
            {
                BlastDbPath = _configuration.GetValue("Blast:BLASTDB", string.Empty),
                Database = "9606/genomic 9606/rna",
                UseIndex = true,
                LowercaseMasking = true,
                OutputFormat = "6 qseqid saccver pident qcovs",
                NumThreads = 4,
            };

            var shortBlastParameters = new BlastParameters
            {
                BlastDbPath = _configuration.GetValue("Blast:BLASTDB", string.Empty),
                Database = "9606/genomic 9606/rna",
                UseIndex = true,
                LowercaseMasking = true,
                OutputFormat = "6 qseqid saccver pident qcovs",
                NumThreads = 4,
                Task = BlastParameters.BlastTask.blastn_short,
                ExpectValue = 1000.0f,
            };

            return query.Length <= 30 ? shortBlastParameters : regularBlastParameters;
        }

        private async Task SendJobCompletionEmail(ApplicationUser user)
        {
            await _emailSender.SendEmailAsync(user.Email, "Job completed", "Your Ribosoft job has completed.");
        }
    }
}
