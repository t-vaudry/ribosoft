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

        /*! \property _activityLogService
         * \brief Activity log service
         */
        private readonly IActivityLogService _activityLogService;

        /*! \property _db
         * \brief Local application database context
         */
        private ApplicationDbContext _db;

        /*! \property RNAStructure
         * \brief RNA structure string
         */
        private String RNAStructure { get; set; } = "";

        /*! \fn GenerateCandidates
         * \brief Default constructor
         * \param options Application database options
         * \param emailSender Email sender
         * \param logger Logging service
         * \param configuration Application configuration
         * \param activityLogService Activity log service
         */
        public GenerateCandidates(DbContextOptions<ApplicationDbContext> options, IEmailSender emailSender, 
                                ILogger<GenerateCandidates> logger, IConfiguration configuration, 
                                IActivityLogService activityLogService)
        {
            _dbOptions = options;
            _db =  new ApplicationDbContext(options);
            _logger = logger;
            _emailSender = emailSender;
            _ribosoftAlgo = new RibosoftAlgo();
            _multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();
            _configuration = configuration;
            _blaster = new Blaster();
            _activityLogService = activityLogService;
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

            // Log job execution start
            await _activityLogService.LogJobExecutionAsync(
                message: $"Job {jobId} Phase1 started - Candidate generation and structure calculation",
                jobId: jobId,
                userId: job.OwnerId,
                userName: job.Owner?.UserName
            );

            // TODO - temporarily catch retried jobs
            await DoStage(job, JobState.Errored, j => j.JobState != JobState.New, async (j, c) => { await Task.CompletedTask; }, cancellationToken);

            // run candidate generator
            await DoStage(job, JobState.CandidateGenerator, j => j.JobState == JobState.New, RunCandidateGenerator, cancellationToken);

            // calculate structure score
            await DoStage(job, JobState.Structure, j => j.JobState == JobState.CandidateGenerator, CalculateStructure, cancellationToken);

            // After structure calculation, queue the appropriate next phase
            // Reload job to get current state after structure calculation
            job = GetJob(jobId);
            
            if (job.JobState == JobState.Structure)
            {
                if (job.TargetEnvironment == TargetEnvironment.InVivo)
                {
                    // InVivo jobs need BLAST analysis (Phase2)
                    await UpdateJobProperties(jobId, JobState.QueuedPhase2, "Queued for BLAST analysis");
                    
                    await _activityLogService.LogJobExecutionAsync(
                        message: $"Job {jobId} Phase1 completed - Queued for Phase2 (BLAST analysis)",
                        jobId: jobId,
                        userId: job.OwnerId,
                        userName: job.Owner?.UserName
                    );
                    
                    BackgroundJob.Enqueue<GenerateCandidates>(x => x.Phase2(jobId, JobCancellationToken.Null));
                }
                else if (job.TargetEnvironment == TargetEnvironment.InVitro)
                {
                    // InVitro jobs skip BLAST and go directly to optimization (Phase3)
                    await UpdateJobProperties(jobId, JobState.QueuedPhase3, "Queued for multi-objective optimization");
                    BackgroundJob.Enqueue<GenerateCandidates>(x => x.Phase3(jobId, JobCancellationToken.Null));
                }
            }
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
            await DoStage(job, JobState.Errored, j => j.JobState != JobState.QueuedPhase2, async (j, c) => { await Task.CompletedTask; }, cancellationToken);

            // run blast to calculate specificity
            await DoStage(job, JobState.Specificity, j => j.JobState == JobState.QueuedPhase2, RunBlast, cancellationToken);
            
            // queue phase 3 job (MOO)
            await DoStage(job, JobState.QueuedPhase3, j => j.JobState == JobState.Specificity, async (j, c) =>
            {
                BackgroundJob.Enqueue<GenerateCandidates>(x => x.Phase3(j.Id, c));
                await Task.CompletedTask;
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
            await DoStage(job, JobState.Errored, j => j.JobState != JobState.QueuedPhase3, async (j, c) => { await Task.CompletedTask; }, cancellationToken);

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
                var statusMessage = GetStatusMessageForState(state);
                await UpdateJobProperties(job.Id, state, statusMessage);
            }

            await func(job, cancellationToken);
        }

        /*! \fn GetStatusMessageForState
         * \brief Gets a descriptive status message for a given job state
         * \param state Job state
         * \return Status message
         */
        private string GetStatusMessageForState(JobState state)
        {
            return state switch
            {
                JobState.New => "Job created and queued for processing",
                JobState.CandidateGenerator => "Generating ribozyme candidates...",
                JobState.Structure => "Calculating structure scores...",
                JobState.Specificity => "Running BLAST analysis for specificity...",
                JobState.MultiObjectiveOptimization => "Optimizing and ranking candidates...",
                JobState.QueuedPhase2 => "Queued for BLAST analysis (Phase 2)",
                JobState.QueuedPhase3 => "Queued for optimization (Phase 3)",
                JobState.Completed => "Job completed successfully",
                JobState.Warning => "Job completed with warnings",
                JobState.Errored => "Job failed with errors",
                JobState.Cancelled => "Job was cancelled",
                _ => $"Job in state: {state}"
            };
        }

        /*! \fn RecreateDbContext
         * \brief Recreates database context object
         */
        private async Task RecreateDbContext()
        {
            _logger.LogInformation("RecreateDbContext: Saving changes and recreating context");
            await _db.SaveChangesAsync();
            var oldContext = _db;
            _db = new ApplicationDbContext(_dbOptions);
            oldContext.Dispose();
            _logger.LogInformation("RecreateDbContext: New context created");
        }

        /*! \fn GetJob
         * \brief Retrieves job object from provided job ID
         * \param jobId Job ID
         * \return Job object
         */
        private Job GetJob(int jobId)
        {
            return _db.Jobs
                .Include(j => j.Assembly)
                .Include(j => j.Ribozyme!)
                    .ThenInclude(r => r.RibozymeStructures)
                .Single(j => j.Id == jobId);
        }

        /*! \fn UpdateJobProperties
         * \brief Safely updates job state and status message without affecting navigation properties
         * \param jobId Job ID
         * \param jobState New job state
         * \param statusMessage New status message
         */
        private async Task UpdateJobProperties(int jobId, JobState jobState, string? statusMessage = null)
        {
            var existingJob = await _db.Jobs
                .Where(j => j.Id == jobId)
                .FirstOrDefaultAsync();
                
            if (existingJob != null)
            {
                existingJob.JobState = jobState;
                if (statusMessage != null)
                {
                    existingJob.StatusMessage = statusMessage;
                }
                
                _db.Entry(existingJob).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
        }

        /*! \fn UpdateJobTolerances
         * \brief Safely updates job tolerance values without affecting navigation properties
         * \param jobId Job ID
         * \param desiredTempTolerance New desired temperature tolerance
         * \param accessibilityTolerance New accessibility tolerance
         */
        private async Task UpdateJobTolerances(int jobId, float? desiredTempTolerance, float? accessibilityTolerance)
        {
            var existingJob = await _db.Jobs
                .Where(j => j.Id == jobId)
                .FirstOrDefaultAsync();
                
            if (existingJob != null)
            {
                existingJob.DesiredTempTolerance = desiredTempTolerance;
                existingJob.AccessibilityTolerance = accessibilityTolerance;
                
                _db.Entry(existingJob).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
        }


        /*! \fn UpdateJobSpecificityTolerance
         * \brief Safely updates job specificity tolerance without affecting navigation properties
         * \param jobId Job ID
         * \param specificityTolerance New specificity tolerance
         */
        private async Task UpdateJobSpecificityTolerance(int jobId, float? specificityTolerance)
        {
            var existingJob = await _db.Jobs
                .Where(j => j.Id == jobId)
                .FirstOrDefaultAsync();
                
            if (existingJob != null)
            {
                existingJob.SpecificityTolerance = specificityTolerance;
                
                _db.Entry(existingJob).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
        }

        /*! \fn UpdateJobSafely
         * \brief Safely updates job properties without affecting navigation properties
         * \param job Job object to update
         */
        private async Task UpdateJobSafely(Job job)
        {
            // Only update the job entity, not its navigation properties
            var existingJob = await _db.Jobs.FindAsync(job.Id);
            if (existingJob != null)
            {
                // Update only the properties we care about, not navigation properties
                existingJob.JobState = job.JobState;
                existingJob.StatusMessage = job.StatusMessage;
                existingJob.DesiredTempTolerance = job.DesiredTempTolerance;
                existingJob.SpecificityTolerance = job.SpecificityTolerance;
                existingJob.AccessibilityTolerance = job.AccessibilityTolerance;
                existingJob.StructureTolerance = job.StructureTolerance;
                
                // No need to call Update or Attach - EF is already tracking existingJob
                await _db.SaveChangesAsync();
            }
        }

        /*! \fn GetJobWithOwner
         * \brief Retrieves job object with owner from provided job ID (only when owner is needed)
         * \param jobId Job ID
         * \return Job object with owner
         */
        private Job GetJobWithOwner(int jobId)
        {
            return _db.Jobs
                .Include(j => j.Owner)
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
                await UpdateJobProperties(job.Id, JobState.Warning, "No Target Region Selected!");
                return;
            }

            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            foreach (var rnaInput in rnaInputs)
            {
                RNAStructure = _ribosoftAlgo.MFEFold(rnaInput);

                foreach (var ribozymeStructure in job.Ribozyme?.RibozymeStructures ?? new List<RibozymeStructure>())
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
                        _logger.LogError(e, "Exception occurred during Candidate Generation.");
                        await UpdateJobProperties(job.Id, JobState.Errored, e.Message);
                        return;
                    }

                    // Algorithms
                    try
                    {
                        uint batchCount = 0;
                        uint totalProcessed = 0;
                        uint totalCandidates = (uint)candidates.Count();
                        _db.ChangeTracker.AutoDetectChangesEnabled = false;

                        foreach (var candidate in candidates)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            RunScoreAlgorithms(candidate, job, ribozymeStructure, RNAStructure);
                            totalProcessed++;

                            if (++batchCount % 100 == 0)
                            {
                                await _db.SaveChangesAsync();
                                await RecreateDbContext();
                                _db.ChangeTracker.AutoDetectChangesEnabled = false;
                                batchCount = 0;
                                
                                // Update progress status
                                var progressMessage = $"Processing candidates: {totalProcessed}/{totalCandidates} completed";
                                await UpdateJobProperties(job.Id, JobState.CandidateGenerator, progressMessage);
                            }
                        }

                        await RecreateDbContext();
                    }
                    catch (RibosoftAlgoException e)
                    {
                        _logger.LogError(e, "Exception occurred during Ribosoft Algorithms.");
                        await UpdateJobProperties(job.Id, JobState.Errored, e.Code.ToString());
                        return;
                    }
                    finally
                    {
                        _db.ChangeTracker.AutoDetectChangesEnabled = true;
                        await UpdateJobProperties(job.Id, job.JobState, job.StatusMessage);
                    }

                    candidateGenerator.Clear();
                }
            }

            _db.ChangeTracker.AutoDetectChangesEnabled = false;
            var designs = _db.Designs.Where(j => j.JobId == job.Id).ToList();

            // Check that there are designs left
            if (!designs.Any())
            {
                _db.ChangeTracker.AutoDetectChangesEnabled = true;
                await UpdateJobProperties(job.Id, JobState.Warning, "No designs returned from Candidate Generation!");
                _logger.LogError("No designs returned from Candidate Generation!");
                return;
            }

            var maxDesiredTemp = designs.Max(d => d.DesiredTemperatureScore.GetValueOrDefault());
            var minDesiredTemp = designs.Min(d => d.DesiredTemperatureScore.GetValueOrDefault());
            var maxAccessibility = designs.Max(d => d.AccessibilityScore.GetValueOrDefault());
            var minAccessibility = designs.Min(d => d.AccessibilityScore.GetValueOrDefault());
            
            var newDesiredTempTolerance = job.DesiredTempTolerance * (maxDesiredTemp - minDesiredTemp);
            var newAccessibilityTolerance = job.AccessibilityTolerance * (maxAccessibility - minAccessibility);
            
            _db.ChangeTracker.AutoDetectChangesEnabled = true;
            await UpdateJobTolerances(job.Id, newDesiredTempTolerance, newAccessibilityTolerance);
            
            // Save all designs to database
            var designCount = designs.Count();
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
        private void RunScoreAlgorithms(Candidate candidate, Job job, RibozymeStructure ribozymeStructure, string RNAStructure)
        {
            var idealStructurePattern = new Regex(@"[^.^(^)]");
            string ideal = idealStructurePattern.Replace(candidate.Structure ?? string.Empty, ".");

            float naConcentration = job.Na.GetValueOrDefault();
            float probeConcentration = job.Probe.GetValueOrDefault();
            float targetTemperature = job.TargetTemperature.GetValueOrDefault();

            var temperatureScore = _ribosoftAlgo.Anneal(candidate, candidate.SubstrateSequence ?? string.Empty,
                candidate.SubstrateStructure ?? string.Empty, naConcentration, probeConcentration, targetTemperature);

            foreach (var cutsiteIndex in candidate.CutsiteIndices ?? new List<int>())
            {
                var accessibilityScore = _ribosoftAlgo.Accessibility(candidate, RNAStructure,
                    cutsiteIndex, naConcentration, probeConcentration, targetTemperature);

                _db.Designs.Add(new Design
                {
                    JobId = job.Id,

                    Sequence = candidate.Sequence?.GetString() ?? string.Empty,
                    IdealStructure = ideal,
                    SubstrateSequence = candidate.SubstrateSequence ?? "",

                    // TODO: save actual cutsite (cutsiteIndex + ribozymeStructure.Cutsite + candidate.CutsiteNumberOffset)
                    CutsiteIndex = cutsiteIndex,

                    SubstrateSequenceLength = candidate.SubstrateSequence?.Length ?? 0,
                    AccessibilityScore = accessibilityScore,
                    DesiredTemperatureScore = temperatureScore
                });
            }
        }

        /*! \fn CalculateStructure
         * \brief Function used to calculate structure score
         * Distance is normalized using scaling to a range based on the highest 
         * distance between structure and ideal strucutre of all candidates
         * \param job Job object
         * \param cancellationToken Cancellation token
         */
        private Task CalculateStructure(Job job, IJobCancellationToken cancellationToken)
        {
            IList<Design> designs = _db.Designs
                             .Where(d => d.JobId == job.Id)
                             .ToList();

            _ribosoftAlgo.Structure(designs);
            
            return Task.CompletedTask;
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
                var designs = _db.Designs.Where(j => j.JobId == job.Id).ToList();
                _multiObjectiveOptimizer.Optimize(designs, 1);
            }
            catch (MultiObjectiveOptimization.MultiObjectiveOptimizationException e)
            {
                _logger.LogError(e, "Exception occurred during Multi Objective Optimization.");
                await UpdateJobProperties(job.Id, JobState.Errored, e.Message);
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
                             .GroupBy(d => new { d.CutsiteIndex, d.SubstrateSequence });

            foreach (var designGroup in designs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var design = designGroup.First();
                var substrateSequence = design.SubstrateSequence;

                if (string.IsNullOrEmpty(substrateSequence))
                {
                    continue;
                }

                // calculate the substrate specificity score, which is common to all designs in this group
                var substrateSpecificityScore = CalculateSpecificity(substrateSequence, job.Assembly?.Path ?? "");

                foreach (var d in designGroup)
                {
                    d.SpecificityScore = substrateSpecificityScore;
                }
            }

            // Specificity is minimized to 1 in the case of a wildtype gene (a score of 0 is ideal for synthetic genes)
            // Anything below 1 means there is absolutely no matching in the organism and will not bond
            // Therefore, remove the design
            if (job.SpecificityMethod == SpecificityMethod.Wildtype)
            {
                _db.Designs.RemoveRange(_db.Designs.Where(d => d.SpecificityScore < 1.0f));
            }

            var completedDesigns = _db.Designs.Where(d => d.JobId == job.Id);

            // Check that there are designs left
            if (!completedDesigns.Any())
            {
                await UpdateJobProperties(job.Id, JobState.Warning, "No designs returned from Candidate Generation!");
                _logger.LogError("No designs returned from Candidate Generation!");
                return;
            }

            float deltaSpecificity = completedDesigns.Max(d => d.SpecificityScore.GetValueOrDefault()) - completedDesigns.Min(d => d.SpecificityScore.GetValueOrDefault());
            var newSpecificityTolerance = job.SpecificityTolerance * deltaSpecificity;
            await UpdateJobSpecificityTolerance(job.Id, newSpecificityTolerance);
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
                BlastDbPath = _configuration.GetValue("Blast:BLASTDB", string.Empty) ?? string.Empty,
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
            // Load the job with owner only when we need to send email
            var jobWithOwner = GetJobWithOwner(job.Id);
            if (jobWithOwner.Owner != null)
            {
                await SendJobCompletionEmail(jobWithOwner.Owner);
            }
        }

        /*! \fn SendJobCompletionEmail
         * \brief Use email sender to send job completion email
         * \param user Current user
         */
        private async Task SendJobCompletionEmail(ApplicationUser user)
        {
            if (!string.IsNullOrEmpty(user.Email))
            {
                await _emailSender.SendEmailAsync(user.Email, "Job completed", "Your Ribosoft job has completed.");
            }
        }
    }
}
