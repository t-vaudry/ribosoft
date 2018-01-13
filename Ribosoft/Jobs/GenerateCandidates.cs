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
        private readonly ApplicationDbContext _db;
        private readonly CandidateGeneration.CandidateGenerator _candidateGenerator;
        private readonly IEmailSender _emailSender;
        private readonly RibosoftAlgo _ribosoftAlgo;
        private readonly MultiObjectiveOptimization.MultiObjectiveOptimizer _multiObjectiveOptimizer;

        public GenerateCandidates(DbContextOptions<ApplicationDbContext> options, IEmailSender emailSender)
        {
            _db =  new ApplicationDbContext(options);
            _candidateGenerator = new CandidateGeneration.CandidateGenerator();
            _emailSender = emailSender;
            _ribosoftAlgo = new RibosoftAlgo();
            _multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task Generate(int jobId, IJobCancellationToken cancellationToken)
        {
            var job = _db.Jobs
                .Include(j => j.Owner)
                .Include(j => j.Ribozyme)
                    .ThenInclude(r => r.RibozymeStructures)
                .Single(j => j.Id == jobId);

            if (job.JobState != JobState.New)
            {
                return;
            }

            job.JobState = JobState.Started;
            await _db.SaveChangesAsync();

            var ribozyme = job.Ribozyme;

            foreach (var ribozymeStructure in ribozyme.RibozymeStructures)
            {
                // Candidate Generation
                try
                {
                    _candidateGenerator.GenerateCandidates(
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
                catch (AggregateException e)
                {
                    job.JobState = JobState.Errored;
                    job.StatusMessage = e.InnerExceptions.FirstOrDefault()?.Message ?? e.Message;
                    await _db.SaveChangesAsync();
                    return;
                }

                // TODO: fix ideal somewhere?
                Regex pattern = new Regex(@"[^.^(^)]");
                string ideal = pattern.Replace(ribozymeStructure.Structure, "."); 

                // Algorithms
                try
                {
                    foreach (var candidate in _candidateGenerator.Candidates)
                    {
                        candidate.FitnessValues[0] = _ribosoftAlgo.Accessibility(candidate, job.RNAInput, ribozymeStructure.SubstrateTemplate, ribozymeStructure.Cutsite); // ACCESSIBILITY
                        candidate.FitnessValues[1] = 0.0f; // NO SPECIFICITY!
                        candidate.FitnessValues[2] = _ribosoftAlgo.Structure(candidate, ideal); // STRUCTURE
                        candidate.FitnessValues[3] = _ribosoftAlgo.Anneal(candidate, job.RNAInput, ribozymeStructure.SubstrateTemplate, 1.0f); // TEMPERATURE
                    }
                }
                catch (RibosoftAlgoException e)
                {
                    job.JobState = JobState.Errored;
                    job.StatusMessage = e.Code.ToString();
                    await _db.SaveChangesAsync();
                    return;
                }

                // Multi-Objective Optimization
                try
                {
                    _multiObjectiveOptimizer.Optimize(
                        _candidateGenerator.Candidates,
                        1);
                }
                catch (MultiObjectiveOptimization.MultiObjectiveOptimizationException e)
                {
                    job.JobState = JobState.Errored;
                    job.StatusMessage = e.Message;
                    await _db.SaveChangesAsync();
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();

                foreach (var candidate in _candidateGenerator.Candidates)
                {
                    var design = new Design
                        {
                            Sequence = candidate.Sequence.GetString(),
                            Rank = candidate.Rank,
                            AccessibilityScore = candidate.FitnessValues[0],
                            SpecificityScore = candidate.FitnessValues[1],
                            StructureScore = candidate.FitnessValues[2],
                            TemperatureScore = candidate.FitnessValues[3]
                        };
                    job.Designs.Add(design);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            job.JobState = JobState.Completed;
            await _db.SaveChangesAsync();

            await SendJobCompletionEmail(job.Owner);
        }

        private async Task SendJobCompletionEmail(ApplicationUser user)
        {
            await _emailSender.SendEmailAsync(user.Email, "Job completed", "Your Ribosoft job has completed.");
        }
    }
}
