using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    public enum JobState
    {
        New,
        Started, // UNUSED
        Completed,
        Cancelled,
        Errored,

        CandidateGenerator,
        MultiObjectiveOptimization,
        Specificity,

        QueuedPhase2,
        QueuedPhase3
    }

    public enum TargetEnvironment
    {
        [Display(Name = "In-vitro")]
        InVitro,
        [Display(Name = "In-vivo")]
        InVivo
    }

    public enum SpecificityMethod
    {
        [Display(Name = "Cleavage")]
        CleavageOnly,
        [Display(Name = "Cleavage and hybridization")]
        CleavageAndHybridization
    }

    public class Job : BaseEntity
    {
        public Job()
        {
            this.Designs = new HashSet<Design>();
        }

        public int Id { get; set; }
        public string OwnerId { get; set; }
        [Display(Name = "Job state")]
        public JobState JobState { get; set; }

        [ScaffoldColumn(false)]
        public int RibozymeId { get; set; }
        [Display(Name = "RNA input")]
        public string RNAInput { get; set; }

        [ScaffoldColumn(false)]
        public string HangfireJobId { get; set; }
        [Display(Name = "Status message")]
        [DisplayFormat(NullDisplayText =  "None")]
        public string StatusMessage { get; set; }

        [Display(Name = "Temperature (℃)")]
        public float? Temperature { get; set; }
        [Display(Name = "Na⁺ (mM)")]
        public float? Na { get; set; }
        [Display(Name = "Probe (nM)")]
        public float? Probe { get; set; }

        [Display(Name = "5'")]
        public bool FivePrime { get; set; }
        [Display(Name = "3'")]
        public bool ThreePrime { get; set; }
        [Display(Name = "Open reading frame")]
        public bool OpenReadingFrame { get; set; }

        [Display(Name = "Open reading frame start index")]
        public int OpenReadingFrameStart { get; set; }
        [Display(Name = "Open reading frame end index")]
        public int OpenReadingFrameEnd { get; set; }

        [Display(Name = "Target environment")]
        public TargetEnvironment TargetEnvironment { get; set; }
        [ScaffoldColumn(false)]
        public int? AssemblyId { get; set; }
        [Display(Name = "Specificity method")]
        public SpecificityMethod? SpecificityMethod { get; set; }

        public virtual ApplicationUser Owner { get; set; }
        public virtual Ribozyme Ribozyme { get; set; }
        public virtual Assembly Assembly { get; set; }

        public virtual ICollection<Design> Designs { get; set; }

        public bool IsInProgress()
        {
            return InProgress().Compile()(this);
        }

        public bool IsCompleted()
        {
            return Completed().Compile()(this);
        }

        public static Expression<Func<Job, bool>> InProgress() =>
            j => j.JobState == JobState.New
                 || j.JobState == JobState.Started
                 || j.JobState == JobState.CandidateGenerator
                 || j.JobState == JobState.Specificity
                 || j.JobState == JobState.MultiObjectiveOptimization
                 || j.JobState == JobState.QueuedPhase2
                 || j.JobState == JobState.QueuedPhase3;

        public static Expression<Func<Job, bool>> Completed() =>
            j => j.JobState == JobState.Completed
                 || j.JobState == JobState.Errored
                 || j.JobState == JobState.Cancelled;
    }
}
