using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Ribosoft.Data;
using Ribosoft.Models;

namespace Ribosoft.Jobs
{
    public class GenerateCandidates
    {
        private readonly ApplicationDbContext _db;
        private readonly CandidateGeneration.CandidateGenerator _candidateGenerator;

        public GenerateCandidates(DbContextOptions<ApplicationDbContext> options)
        {
            _db =  new ApplicationDbContext(options);
            _candidateGenerator = new CandidateGeneration.CandidateGenerator();
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task Generate(int jobId, IJobCancellationToken cancellationToken)
        {
            var job = _db.Jobs
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
                catch (CandidateGeneration.CandidateGenerationException)
                {
                    job.JobState = JobState.Errored;
                    await _db.SaveChangesAsync();
                    throw;
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
        }
    }
}
