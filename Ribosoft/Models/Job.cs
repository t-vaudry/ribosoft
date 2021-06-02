using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    /*! \enum JobState
     * \brief State codes
     * New, Started (unused), Completed, Cancelled, Warning, Errored, CandidateGenerator, MultiObjectiveOptimization, Specificity, QueuedPhase2, QueuedPhase3
     */
    public enum JobState
    {
        New,
        Started, // UNUSED
        Completed,
        Cancelled,
        Warning,
        Errored,

        CandidateGenerator,
        MultiObjectiveOptimization,
        Specificity,

        QueuedPhase2,
        QueuedPhase3
    }

    /*! \enum TargetEnvironment
     * \brief Target environments
     * In-vitro OR In-vivo
     */
    public enum TargetEnvironment
    {
        [Display(Name = "In-vitro")]
        InVitro,
        [Display(Name = "In-vivo")]
        InVivo
    }

    /*! \enum SpecificityMethod
     * \brief Methods
     * Cleavage OR Cleavage and hybridization
     */
    public enum SpecificityMethod
    {
        [Display(Name = "Cleavage")]
        CleavageOnly,
        [Display(Name = "Cleavage and hybridization")]
        CleavageAndHybridization
    }

    /*! \class Job
     * \brief Model class for a job
     */
    public class Job : BaseEntity
    {
        /*! \fn Job
         * \brief Default constructor
         */
        public Job()
        {
            this.Designs = new HashSet<Design>();
        }

        /*! \property Id
         * \brief Job ID
         */
        [Display(Name = "Job Id")]
        public int Id { get; set; }

        /*! \property OwnerId
         * \brief Owner ID
         */
        public string OwnerId { get; set; }

        /*! \property JobState
         * \brief Job state
         */
        [Display(Name = "Job state")]
        public JobState JobState { get; set; }

        /*! \property RibozymeId
         * \brief Ribozyme ID
         */
        [ScaffoldColumn(false)]
        public int RibozymeId { get; set; }

        /*! \property RNAInput
         * \brief RNA input
         */
        [Display(Name = "RNA input")]
        public string RNAInput { get; set; }

        /*! \property HangfireJobId
         * \brief Hangfire Job ID
         */
        [ScaffoldColumn(false)]
        public string HangfireJobId { get; set; }

        /*! \property StatusMessage
         * \brief Status message
         */
        [Display(Name = "Status message")]
        [DisplayFormat(NullDisplayText =  "None")]
        public string StatusMessage { get; set; }

        /*! \property Temperature
         * \brief Temperature (℃)
         */
        [Display(Name = "Temperature (℃)")]
        public float? Temperature { get; set; }

        /*! \property Na
         * \brief Na⁺ (mM) concentration
         */
        [Display(Name = "Na⁺ (mM)")]
        public float? Na { get; set; }

        /*! \property Probe
         * \brief Probe (nM) concentration
         */
        [Display(Name = "Probe (nM)")]
        public float? Probe { get; set; }

        /*! \property FivePrime
         * \brief 5' selected
         */
        [Display(Name = "5'")]
        public bool FivePrime { get; set; }

        /*! \property ThreePrime
         * \brief 3' selected
         */
        [Display(Name = "3'")]
        public bool ThreePrime { get; set; }

        /*! \property OpenReadingFrame
         * \brief Open reading frame selected
         */
        [Display(Name = "Open reading frame")]
        public bool OpenReadingFrame { get; set; }

        /*! \property OpenReadingFrameStart
         * \brief Index of the start of the open reading frame
         */
        [Display(Name = "Open reading frame start index")]
        public int OpenReadingFrameStart { get; set; }

        /*! \property OpenReadingFrameEnd
         * \brief Index of the end of the open reading frame
         */
        [Display(Name = "Open reading frame end index")]
        public int OpenReadingFrameEnd { get; set; }

        [Display(Name = "Snake Sequence")]
        public string SnakeSequence { get; set; }

        [Display(Name = "Stem I Temperature (℃)")]
        public float? StemITemperature { get; set; }

        [Display(Name = "Stem III Temperature (℃)")]
        public float? StemIIITemperature { get; set; }

        [Display(Name = "Lower StemII Length")]
        public int? LowerStemIILength { get; set; }

        [Display(Name = "Bulge Length")]
        public int? BulgeLength { get; set; }

        [Display(Name = "Upper StemII Length")]
        public int? UpperStemIILength { get; set; }

        [Display(Name = "Loop Length")]
        public int? LoopLength { get; set; }

        /*! \property TargetEnvironment
         * \brief Target environment
         */
        [Display(Name = "Target environment")]
        public TargetEnvironment TargetEnvironment { get; set; }

        /*! \property AssemblyId
         * \brief Assembly ID
         */
        [ScaffoldColumn(false)]
        public int? AssemblyId { get; set; }

        /*! \property SpecificityMethod
         * \brief Specificity method
         */
        [Display(Name = "Specificity method")]
        public SpecificityMethod? SpecificityMethod { get; set; }

        /*! \property DesiredTempTolerance
         * \brief Desired Temperature Tolerance
         */
        [Display(Name = "Desired Temperature Tolerance")]
        public float? DesiredTempTolerance { get; set; }

        /*! \property HighestTempTolerance
         * \brief Highest Temperature Tolerance
         */
        [Display(Name = "Highest Temperature Tolerance")]
        public float? HighestTempTolerance { get; set; }

        /*! \property SpecificityTolerance
         * \brief Specificity Tolerance
         */
        [Display(Name = "Specificity Tolerance")]
        public float? SpecificityTolerance { get; set; }

        /*! \property AccessibilityTolerance
         * \brief Accessibility Tolerance
         */
        [Display(Name = "Accessibility Tolerance")]
        public float? AccessibilityTolerance { get; set; }

        /*! \property StructureTolerance
         * \brief Structure Tolerance
         */
        [Display(Name = "Structure Tolerance")]
        public float? StructureTolerance { get; set; }

        /*! \property Owner
         * \brief Application owner
         */
        public virtual ApplicationUser Owner { get; set; }

        /*! \property Ribozyme
         * \brief Ribozyme object
         */
        public virtual Ribozyme Ribozyme { get; set; }

        /*! \property Assembly
         * \brief Assembly object
         */
        public virtual Assembly Assembly { get; set; }

        /*! \property Designs
         * \brief Collection of designs
         */
        public virtual ICollection<Design> Designs { get; set; }

        /*! \fn IsInProgress
         * \brief Check if job is in progress
         * \return Boolean result from the check
         */
        public bool IsInProgress()
        {
            return InProgress().Compile()(this);
        }

        /*! \fn IsCompleted
         * \brief Check if job is completed
         * \return Boolean result from the check
         */
        public bool IsCompleted()
        {
            return Completed().Compile()(this);
        }

        /*! \static InProgress
         * \brief Function to express in-progress jobs
         * \return Expression of job functions
         */
        public static Expression<Func<Job, bool>> InProgress() =>
            j => j.JobState == JobState.New
                 || j.JobState == JobState.Started
                 || j.JobState == JobState.CandidateGenerator
                 || j.JobState == JobState.Specificity
                 || j.JobState == JobState.MultiObjectiveOptimization
                 || j.JobState == JobState.QueuedPhase2
                 || j.JobState == JobState.QueuedPhase3;

        /*! \static Completed
         * \brief Function to express completed jobs
         * \return Expression of job functions
         */
        public static Expression<Func<Job, bool>> Completed() =>
            j => j.JobState == JobState.Completed
                 || j.JobState == JobState.Errored
                 || j.JobState == JobState.Warning
                 || j.JobState == JobState.Cancelled;
    }
}
