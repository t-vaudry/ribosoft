using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [DisplayName("Job State")]
        public JobState JobState { get; set; }

        public int RibozymeId { get; set; }
        [DisplayName("RNA Input")]
        public string RNAInput { get; set; }
        [ScaffoldColumn(false)]
        public string HangfireJobId { get; set; }

        public virtual ApplicationUser Owner { get; set; }
        public virtual Ribozyme Ribozyme { get; set; }

        public virtual ICollection<Design> Designs { get; set; }
    }
}
