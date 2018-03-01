using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Ribosoft.Data;
using Ribosoft.Models;
using Ribosoft.Services;

namespace Ribosoft.Jobs
{
    public class GenerateCandidates
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IEmailSender _emailSender;
        private readonly RibosoftAlgo _ribosoftAlgo;
        private readonly MultiObjectiveOptimization.MultiObjectiveOptimizer _multiObjectiveOptimizer;

        private ApplicationDbContext _db;

        public GenerateCandidates(DbContextOptions<ApplicationDbContext> options, IEmailSender emailSender)
        {
            _dbOptions = options;
            _db =  new ApplicationDbContext(options);
            _emailSender = emailSender;
            _ribosoftAlgo = new RibosoftAlgo();
            _multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task Generate(int jobId, IJobCancellationToken cancellationToken)
        {
            var job = GetJob(jobId);

            if (job.JobState != JobState.New)
            {
                return;
            }

            job.JobState = JobState.Started;
            await _db.SaveChangesAsync();

            await RunCandidateGenerator(job, cancellationToken);
            await MultiObjectiveOptimize(job, cancellationToken);

            job.JobState = JobState.Completed;
            await _db.SaveChangesAsync();

            await SendJobCompletionEmail(job.Owner);
        }

        private void RecreateDbContext()
        {
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
            uint batchCount = 0;
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
                        _db.ChangeTracker.AutoDetectChangesEnabled = false;

                        foreach (var candidate in candidates)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            string ideal = idealStructurePattern.Replace(candidate.Structure, ".");

                            var accessibilityScore = _ribosoftAlgo.Accessibility(candidate, job.RNAInput,
                                ribozymeStructure.Cutsite + candidate.CutsiteNumberOffset);
                            var specificityScore = 0.0f; // TODO
                            var structureScore = _ribosoftAlgo.Structure(candidate, ideal);
                            var temperatureScore = _ribosoftAlgo.Anneal(candidate, candidate.SubstrateSequence,
                                candidate.SubstrateStructure, job.Na.GetValueOrDefault(), job.Probe.GetValueOrDefault());

                            _db.Designs.Add(new Design
                            {
                                JobId = job.Id,

                                Sequence = candidate.Sequence.GetString(),
                                AccessibilityScore = accessibilityScore,
                                SpecificityScore = specificityScore,
                                StructureScore = structureScore,
                                HighestTemperatureScore = -1.0f * temperatureScore,
                                DesiredTemperatureScore = Math.Abs(temperatureScore - job.Temperature.GetValueOrDefault())
                            });

                            if (batchCount % 100 == 0)
                            {
                                await _db.SaveChangesAsync();
                                RecreateDbContext();
                                _db.ChangeTracker.AutoDetectChangesEnabled = false;
                                batchCount = 0;
                            }
                        }
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
                await _db.SaveChangesAsync();
            }
        }

        private async Task SendJobCompletionEmail(ApplicationUser user)
        {
            await _emailSender.SendEmailAsync(user.Email, "Job completed", "Your Ribosoft job has completed.");
        }
    }
}
