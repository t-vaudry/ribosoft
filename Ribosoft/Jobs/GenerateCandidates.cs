using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ribosoft.Data;
using Ribosoft.Models;
using Ribosoft.Services;
using Ribosoft.Blast;
using Microsoft.Extensions.Configuration;
using System.Text;
using Ribosoft.Biology;

namespace Ribosoft.Jobs
{
    /*! \class GenerateCandidates
     * \brief Job class for the entire core of Ribosoft functionality, generating candidates, evaluation functions and multi-objective optimization
     */
    public class GenerateCandidates
    {
        /*! \property _dbOptions
         * \brief Local application database options
         */
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        /*! \property _logger
         * \brief Local logging service
         */
        private readonly ILogger<GenerateCandidates> _logger;

        /*! \property _emailSender
         * \brief Local email sender
         */
        private readonly IEmailSender _emailSender;

        /*! \property _ribosoftAlgo
         * \brief Local reference to RibosoftAlgo library
         */
        private readonly RibosoftAlgo _ribosoftAlgo;

        /*! \property _multiObjectiveOptimizer
         * \brief Local object of multi-objective optimizer
         */
        private readonly MultiObjectiveOptimization.MultiObjectiveOptimizer _multiObjectiveOptimizer;

        /*! \property _configuration
         * \brief Local application configuration
         */
        private readonly IConfiguration _configuration;

        /*! \property _blaster
         * \brief Local object of BLAST command tool
         */
        private readonly Blaster _blaster;

        /*! \property _db
         * \brief Local application database context
         */
        private ApplicationDbContext _db;

        /*! \fn GenerateCandidates
         * \brief Default constructor
         * \param options Application database options
         * \param emailSender Email sender
         * \param logger Logging service
         * \param configuration Application configuration
         */
        public GenerateCandidates(DbContextOptions<ApplicationDbContext> options, IEmailSender emailSender, ILogger<GenerateCandidates> logger, IConfiguration configuration)
        {
            _dbOptions = options;
            _db =  new ApplicationDbContext(options);
            _logger = logger;
            _emailSender = emailSender;
            _ribosoftAlgo = new RibosoftAlgo();
            _multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();
            _configuration = configuration;
            _blaster = new Blaster();
        }

        /*! \fn Phase1
         * \brief Phase one of the software
         * Will submit jobs to begin the candidate generation
         * Followed by being queued for Phase 2 (BLAST queries, if in-vivo)
         * OR straight to Phase 3 (Multi-objective optimization, if in-vitro)
         * \param jobId Job ID
         * \param cancellationToken Cancellation token
         */
        [AutomaticRetry(Attempts = 0)]
        public async Task Phase1(int jobId, IJobCancellationToken cancellationToken)
        {
            var job = GetJob(jobId);

            // TODO - temporarily catch retried jobs
            await DoStage(job, JobState.Errored, j => j.JobState != JobState.New, async (j, c) => { await Task.FromResult<object>(null); }, cancellationToken);

            // run candidate generator
            await DoStage(job, JobState.CandidateGenerator, j => j.JobState == JobState.New, RunCandidateGenerator, cancellationToken);
            
            // queue phase 2 job for in-vivo runs (blast)
            await DoStage(job, JobState.QueuedPhase2, j => j.JobState == JobState.CandidateGenerator && j.TargetEnvironment == TargetEnvironment.InVivo, async (j, c) =>
                {
                    BackgroundJob.Enqueue<GenerateCandidates>(x => x.Phase2(j.Id, c));
                    await Task.FromResult<object>(null);
                }, cancellationToken);
            
            // queue phase 3 job for in-vitro runs, skipping phase 2 (MOO)
            await DoStage(job, JobState.QueuedPhase3, j => j.JobState == JobState.CandidateGenerator && j.TargetEnvironment == TargetEnvironment.InVitro, async (j, c) =>
            {
                BackgroundJob.Enqueue<GenerateCandidates>(x => x.Phase3(j.Id, c));
                await Task.FromResult<object>(null);
            }, cancellationToken);
        }

        /*! \fn Phase2
         * \brief Phase two of the software
         * Will submit jobs to calculate specificity
         * Followed by being queued for Phase 3 (Multi-objective optimization)
         * \param jobId Job ID
         * \param cancellationToken Cancellation token
         */
        [Queue("blast")]
        [AutomaticRetry(Attempts = 0)]
        public async Task Phase2(int jobId, IJobCancellationToken cancellationToken)
        {
            var job = GetJob(jobId);
            
            // TODO - temporarily catch retried jobs
            await DoStage(job, JobState.Errored, j => j.JobState != JobState.QueuedPhase2, async (j, c) => { await Task.FromResult<object>(null); }, cancellationToken);

            // run blast to calculate specificity
            await DoStage(job, JobState.Specificity, j => j.JobState == JobState.QueuedPhase2, RunBlast, cancellationToken);
            
            // queue phase 3 job (MOO)
            await DoStage(job, JobState.QueuedPhase3, j => j.JobState == JobState.Specificity, async (j, c) =>
            {
                BackgroundJob.Enqueue<GenerateCandidates>(x => x.Phase3(j.Id, c));
                await Task.FromResult<object>(null);
            }, cancellationToken);
        }

        /*! \fn Phase3
         * \brief Phase three of the software
         * Will submit jobs to multi-objective optimization
         * \param jobId Job ID
         * \param cancellationToken Cancellation token
         */
        [AutomaticRetry(Attempts = 0)]
        public async Task Phase3(int jobId, IJobCancellationToken cancellationToken)
        {
            var job = GetJob(jobId);
            
            // TODO - temporarily catch retried jobs
            await DoStage(job, JobState.Errored, j => j.JobState != JobState.QueuedPhase3, async (j, c) => { await Task.FromResult<object>(null); }, cancellationToken);

            // run multi-objective optimization
            await DoStage(job, JobState.MultiObjectiveOptimization, j => j.JobState == JobState.QueuedPhase3, MultiObjectiveOptimize, cancellationToken);
            
            // complete job
            await DoStage(job, JobState.Completed, j => j.JobState == JobState.MultiObjectiveOptimization, CompleteJob, cancellationToken); 
        }

        /*! \fn DoStage
         * \brief Function that performs the job
         * \param job Job object
         * \param state Job state
         * \param acceptFunc Acceptance function
         * \param func Function to perform the job
         * \param cancellationToken Cancellation token
         */
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

        /*! \fn RecreateDbContext
         * \brief Recreates database context object
         */
        private async Task RecreateDbContext()
        {
            await _db.SaveChangesAsync();
            _db = new ApplicationDbContext(_dbOptions);
        }

        /*! \fn GetJob
         * \brief Retrieves job object from provided job ID
         * \param jobId Job ID
         * \return Job object
         */
        private Job GetJob(int jobId)
        {
            return _db.Jobs
                .Include(j => j.Owner)
                .Include(j => j.Assembly)
                .Include(j => j.Ribozyme)
                    .ThenInclude(r => r.RibozymeStructures)
                .Single(j => j.Id == jobId);
        }

        /*! \fn RunCandidateGenerator
         * \brief Function used to run candidate generation
         * Generated candidates are then evaluated using the RibosoftAlgo library
         * Finally candidates move onto Phase 2 (if in-vivo) or Phase 3 (if in-vitro)
         * \param job Job object
         * \param cancellationToken Cancellation token
         */
        private async Task RunCandidateGenerator(Job job, IJobCancellationToken cancellationToken)
        {
            List<string> rnaInputs = new List<string>();
            if (job.FivePrime || job.OpenReadingFrame || job.ThreePrime)
            {
                SetTargetRegions(job, ref rnaInputs);
            }
            else
            {
                job.JobState = JobState.Warning;
                job.StatusMessage = "No Target Region Selected!";
                await _db.SaveChangesAsync();
                return;
            }

            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            foreach (var rnaInput in rnaInputs)
            {
                foreach (var ribozymeStructure in job.Ribozyme.RibozymeStructures)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    IEnumerable<Candidate> candidates;
                    try
                    {
                        if (String.IsNullOrEmpty(job.SnakeSequence))
                        {
                            // Candidate Generation
                            candidates = candidateGenerator.GenerateCandidates(
                                ribozymeStructure.Sequence,
                                ribozymeStructure.Structure,
                                ribozymeStructure.SubstrateTemplate,
                                ribozymeStructure.SubstrateStructure,
                                rnaInput);
                        }
                        else
                        {
                            string startSearchString = "CUGAUGA";
                            int startSearchStringPosition = ribozymeStructure.Sequence.IndexOf(startSearchString);
                            int startSearchStringLength = startSearchString.Length;

                            string endSearchString = "GAAAC";
                            int endSearchStringPosition = ribozymeStructure.Sequence.IndexOf(endSearchString);
                            int endSearchStringLength = endSearchString.Length;

                            string modifiedSequence = ribozymeStructure.Sequence.Substring(0, startSearchStringPosition + startSearchStringLength);
                            string modifiedStructure = ribozymeStructure.Structure.Substring(0, startSearchStringPosition + startSearchStringLength);

                            modifiedSequence += job.SnakeSequence.Substring(0, job.LowerStemIILength.Value);

                            modifiedSequence += job.SnakeSequence.Substring(job.LowerStemIILength.Value, job.BulgeLength.Value).Replace('A', 'B').Replace('C', 'D').Replace('G', 'H').Replace('U', 'V');

                            modifiedSequence += job.SnakeSequence.Substring(job.LowerStemIILength.Value + job.BulgeLength.Value, job.UpperStemIILength.Value);

                            //add loop not part of snake to sequence
                            modifiedSequence += new String(
                                'N',
                                job.LowerStemIILength.Value +
                                job.BulgeLength.Value +
                                job.UpperStemIILength.Value +
                                job.LoopLength.Value -
                                job.SnakeSequence.Length);

                            char[] reverseArray = job.SnakeSequence.ToCharArray();
                            Array.Reverse(reverseArray);

                            string complementSnake = new string(reverseArray);

                            for (int i = 0; i < complementSnake.Length; i++)
                            {
                                char currentChar = complementSnake[i];

                                if (currentChar == 'A')
                                    complementSnake = complementSnake.Remove(i, 1).Insert(i, "U");
                                else if (currentChar == 'C')
                                    complementSnake = complementSnake.Remove(i, 1).Insert(i, "G");
                                else if (currentChar == 'G')
                                    complementSnake = complementSnake.Remove(i, 1).Insert(i, "C");
                                else if (currentChar == 'U')
                                    complementSnake = complementSnake.Remove(i, 1).Insert(i, "A");
                            }

                            modifiedSequence += complementSnake;

                            modifiedStructure +=
                                new String('(', job.LowerStemIILength.Value) +
                                new String('.', job.BulgeLength.Value) +
                                new String('(', job.UpperStemIILength.Value) +
                                new String('.', job.LoopLength.Value) +
                                new String(')', job.UpperStemIILength.Value) +
                                new String('.', job.BulgeLength.Value) +
                                new String(')', job.LowerStemIILength.Value);

                            modifiedSequence += ribozymeStructure.Sequence.Substring(endSearchStringPosition);
                            modifiedStructure += ribozymeStructure.Structure.Substring(endSearchStringPosition);

                            candidates = candidateGenerator.GenerateCandidates(
                                modifiedSequence,
                                modifiedStructure,
                                ribozymeStructure.SubstrateTemplate,
                                ribozymeStructure.SubstrateStructure,
                                rnaInput);
                        }
                    }
                    catch (CandidateGeneration.CandidateGenerationException e)
                    {
                        job.JobState = JobState.Errored;
                        job.StatusMessage = e.Message;
                        _logger.LogError(e, "Exception occurred during Candidate Generation.");
                        await _db.SaveChangesAsync();
                        return;
                    }

                    // Algorithms
                    try
                    {
                        uint batchCount = 0;
                        _db.ChangeTracker.AutoDetectChangesEnabled = false;

                        int testingAnk = 0;

                        foreach (var candidate in candidates)
                        {
                            testingAnk++;

                            cancellationToken.ThrowIfCancellationRequested();
                            RunScoreAlgorithms(candidate, job, ribozymeStructure);

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
                        _logger.LogError(e, "Exception occurred during Ribosoft Algorithms.");
                        return;
                    }
                    finally
                    {
                        _db.ChangeTracker.AutoDetectChangesEnabled = true;
                        _db.Jobs.Attach(job);
                        await _db.SaveChangesAsync();
                    }

                    candidateGenerator.Clear();
                }
            }

            _db.ChangeTracker.AutoDetectChangesEnabled = false;
            var designs = _db.Designs.Where(j => j.JobId == job.Id).ToList();

            // Check that there are designs left
            if (!designs.Any())
            {
                job.JobState = JobState.Warning;
                job.StatusMessage = "No designs returned from Candidate Generation!";
                _logger.LogError("No designs returned from Candidate Generation!");
                _db.ChangeTracker.AutoDetectChangesEnabled = true;
                _db.Jobs.Attach(job);
                await _db.SaveChangesAsync();
                return;
            }

            job.DesiredTempTolerance *= designs.Max(d => d.DesiredTemperatureScore.GetValueOrDefault()) - designs.Min(d => d.DesiredTemperatureScore.GetValueOrDefault());
            job.HighestTempTolerance *= designs.Max(d => d.HighestTemperatureScore.GetValueOrDefault()) - designs.Min(d => d.HighestTemperatureScore.GetValueOrDefault());
            job.AccessibilityTolerance *= designs.Max(d => d.AccessibilityScore.GetValueOrDefault()) - designs.Min(d => d.AccessibilityScore.GetValueOrDefault());
            job.StructureTolerance *= designs.Max(d => d.StructureScore.GetValueOrDefault()) - designs.Min(d => d.StructureScore.GetValueOrDefault());
            job.MalformationTolerance *= designs.Max(d => d.MalformationScore.GetValueOrDefault()) - designs.Min(d => d.MalformationScore.GetValueOrDefault());

            _db.ChangeTracker.AutoDetectChangesEnabled = true;
            _db.Jobs.Attach(job);
            await _db.SaveChangesAsync();
        }

        /*! \fn SetTargetRegions
         * \brief Helper function to set the target regions for the job
         * \param job Current job
         * \param rnaInputs List of RNA inputs
         */
        private void SetTargetRegions(Job job, ref List<string> rnaInputs)
        {
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
        }

        /*! \fn RunScoreAlgorithms
         * \brief Helper function to run score algorithms on candidates
         * \param candidate Current candidate
         * \param job Current job
         * \param ribozymeStructure Current ribozyme structure
         */
        private void RunScoreAlgorithms(Candidate candidate, Job job, RibozymeStructure ribozymeStructure)
        {
            var idealStructurePattern = new Regex(@"[^.^(^)]");
            string ideal = idealStructurePattern.Replace(candidate.Structure, ".");

            var temperatureScore = _ribosoftAlgo.Anneal(candidate, candidate.SubstrateSequence,
                candidate.SubstrateStructure, job.Na.GetValueOrDefault(), job.Probe.GetValueOrDefault());
            /*if (temperatureScore < 0.0f || temperatureScore > 100.0f)
            {
                continue;
            }*/

            var accessibilityScore = _ribosoftAlgo.Accessibility(candidate, job.RNAInput,
                ribozymeStructure.Cutsite + candidate.CutsiteNumberOffset);

            if (String.IsNullOrEmpty(job.SnakeSequence))
            {
                string dummy = "";
                var structureScore = _ribosoftAlgo.Structure(candidate, ideal, ref dummy);

                _db.Designs.Add(new Design
                {
                    JobId = job.Id,

                    Sequence = candidate.Sequence.GetString(),
                    CutsiteIndex = candidate.CutsiteIndices.First(),
                    SubstrateSequenceLength = candidate.SubstrateSequence.Length,

                    AccessibilityScore = accessibilityScore,
                    StructureScore = structureScore,
                    HighestTemperatureScore = temperatureScore,
                    DesiredTemperatureScore = Math.Abs(temperatureScore - job.Temperature.GetValueOrDefault())
                });
            }
            else
            {
                string endSearchString = "GAAAC";
                int endSearchStringPosition = candidate.Sequence.GetString().IndexOf(endSearchString);

                string structureStructure = "";
                var structureScore = _ribosoftAlgo.Structure(candidate, ideal, ref structureStructure);

                string malformationStructure = "";
                var malformationScore = _ribosoftAlgo.Malformation(
                    candidate,
                    ideal,
                    ref malformationStructure,
                    endSearchStringPosition - job.SnakeSequence.Length,
                    job.SnakeSequence.Length);

                _db.Designs.Add(new Design
                {
                    JobId = job.Id,

                    Sequence = candidate.Sequence.GetString(),
                    Structure = structureStructure,
                    CutsiteIndex = candidate.CutsiteIndices.First(),
                    SubstrateSequenceLength = candidate.SubstrateSequence.Length,

                    AccessibilityScore = accessibilityScore,
                    StructureScore = structureScore,
                    MalformationScore = malformationScore,
                    MalformationStructure = malformationStructure,
                    HighestTemperatureScore = temperatureScore,
                    DesiredTemperatureScore = Math.Abs(temperatureScore - job.Temperature.GetValueOrDefault())
                });
            }
        }

        /*! \fn MultiObjectiveOptimize
         * \brief Function used to run multi-objective optimization
         * Candidates are ranked and stored in the database
         * \param job Job object
         * \param cancellationToken Cancellation token
         */
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
                _logger.LogError(e, "Exception occurred during Multi Objective Optimization.");
            }
            finally
            {
                await _db.SaveChangesAsync();
            }
        }

        /*! \fn RunBlast
         * \brief Function used to run BLAST commands
         * Results are used to calculate specificity of candidates
         * Finally, they move onto Phase 3 (Multi-objective optimization)
         * \param job Job object
         * \param cancellationToken Cancellation token
         */
        private async Task RunBlast(Job job, IJobCancellationToken cancellationToken)
        {
            // check if blastn is available; if it isn't, ignore specificity
            if (!_blaster.IsAvailable())
            {
            	_logger.LogWarning("RibosoftWarning | BLAST Service is not available!!");
                return;
            }
            
            var designs = _db.Designs
                             .Where(d => d.JobId == job.Id)
                             .AsEnumerable()
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

                // calculate the substrate specificity score, which is common to all designs in this group
                var substrateSpecificityScore = CalculateSpecificity(substrateSequence, job.Assembly.Path);
                
                if (job.SpecificityMethod == SpecificityMethod.CleavageAndHybridization)
                {
                    // if we're doing hybridization specificity, also compute the score for the design sequence complement
                    foreach (var d in designGroup)
                    {
                        d.SpecificityScore = substrateSpecificityScore + CalculateSpecificity(new Sequence(d.Sequence).GetComplement(), job.Assembly.Path);
                    }
                }
                else
                {
                    // for cleavage-only specificity, only use the substrate specificity score
                    foreach (var d in designGroup)
                    {
                        d.SpecificityScore = substrateSpecificityScore;
                    }
                }
            }

            // Specificity is minimized to 1
            // Anything below 1 means there is absolutely no matching in the organism and will not bond
            // Therefore, remove the design
            _db.Designs.RemoveRange(_db.Designs.Where(d => d.SpecificityScore < 1.0f));

            var completedDesigns = _db.Designs.Where(d => d.JobId == job.Id);

            // Check that there are designs left
            if (!completedDesigns.Any())
            {
                job.JobState = JobState.Warning;
                job.StatusMessage = "No designs returned from Candidate Generation!";
                _logger.LogError("No designs returned from Candidate Generation!");
                _db.Jobs.Attach(job);
                await _db.SaveChangesAsync();
                return;
            }

            float deltaSpecificity = completedDesigns.Max(d => d.SpecificityScore.GetValueOrDefault()) - completedDesigns.Min(d => d.SpecificityScore.GetValueOrDefault());
            job.SpecificityTolerance *= deltaSpecificity;
            _db.Jobs.Attach(job);

            await _db.SaveChangesAsync();
        }

        /*! \fn CalculateSpecificity
         * \brief Function to calculate specificity of a given sequence
         * \param sequence Substrate sequence
         * \param database BLAST database
         * \return SpecificityScore
         */
        private float CalculateSpecificity(string sequence, string database)
        {
            var specificityScore = 0.0f;
            var blastParameters = BlastParametersForQuery(database, sequence);
            blastParameters.Query = sequence;
            var output = _blaster.Run(blastParameters);

            foreach (var line in output.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
            {
                var fields = line.Split('\t');

                // filter out X_ predictions
                //if (fields[1].StartsWith('X'))
                //{
                //    continue;
                //}

                specificityScore += float.Parse(fields[2]) * float.Parse(fields[3]) / 10000;
            }

            return specificityScore;
        }

        /*! \fn BlastParametersForQuery
         * \brief Retrieve BLAST parameters for given query
         * \param database BLAST database
         * \param query BLAST query
         * \return BLAST parameters
         */
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

        /*! \fn CompleteJob
         * \brief Final phase to complete job and send email confirmation
         * \param job Job object
         * \param cancellationToken Cancellation token
         */
        private async Task CompleteJob(Job job, IJobCancellationToken cancellationToken)
        {
            await SendJobCompletionEmail(job.Owner);
        }

        /*! \fn SendJobCompletionEmail
         * \brief Use email sender to send job completion email
         * \param user Current user
         */
        private async Task SendJobCompletionEmail(ApplicationUser user)
        {
            await _emailSender.SendEmailAsync(user.Email, "Job completed", "Your Ribosoft job has completed.");
        }
    }
}
