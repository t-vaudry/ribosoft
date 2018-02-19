using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    public enum JobState
    {
        New,
        Started,
        Completed,
        Cancelled,
        Errored
    }

    public class Job : BaseEntity
    {
        public Job()
        {
            this.Designs = new HashSet<Design>();
        }

        public int Id { get; set; }
        public string OwnerId { get; set; }
        [Display(Name = "Job State")]
        public JobState JobState { get; set; }
        public int RibozymeId { get; set; }
        [Display(Name = "RNA Input")]
        public string RNAInput { get; set; }
        [Display(Name = "Open Reading Frame Start Index")]
        public int OpenReadingFrameStart { get; set; }
        [Display(Name = "Open Reading Frame End Index")]
        public int OpenReadingFrameEnd { get; set; }
        [ScaffoldColumn(false)]
        public string HangfireJobId { get; set; }
        [Display(Name = "Status Message")]
        [DisplayFormat(NullDisplayText =  "None")]
        public string StatusMessage { get; set; }
        [Display(Name = "Temperature")]
        public float? Temperature { get; set; }
        [Display(Name = "Na")]
        public float? Na { get; set; }
        [Display(Name = "Probe")]
        public float? Probe { get; set; }
        [Display(Name = "Target Regions")]
        public TargetRegion[] TargetRegions { get; set; }

        public virtual ApplicationUser Owner { get; set; }
        public virtual Ribozyme Ribozyme { get; set; }

        public virtual ICollection<Design> Designs { get; set; }
    }
}
