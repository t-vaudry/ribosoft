using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    public class RibozymeStructure : BaseEntity
    {
        public int Id { get; set; }
        public int RibozymeId { get; set; }
        public int Cutsite { get; set; }
        public string Sequence { get; set; }
        public string Structure { get; set; }
        [Display(Name = "Substrate Template")]
        public string SubstrateTemplate { get; set; }
        [Display(Name = "Substrate Structure")]
        public string SubstrateStructure { get; set; }
        [Display(Name = "Post Process")]
        public bool PostProcess { get; set; }

        public virtual Ribozyme Ribozyme { get; set; }
    }
}
