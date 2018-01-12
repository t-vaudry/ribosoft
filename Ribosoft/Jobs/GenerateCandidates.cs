using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public GenerateCandidates(DbContextOptions<ApplicationDbContext> options, IEmailSender emailSender)
        {
            _db =  new ApplicationDbContext(options);
            _candidateGenerator = new CandidateGeneration.CandidateGenerator();
            _emailSender = emailSender;
            _ribosoftAlgo = new RibosoftAlgo();
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

                cancellationToken.ThrowIfCancellationRequested();

                foreach (var sequence in _candidateGenerator.SequencesToSend)
                {
                    var design = new Design { Sequence = sequence.GetString() };
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
