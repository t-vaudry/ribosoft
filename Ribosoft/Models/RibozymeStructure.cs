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
        [Required]
        [DataType(DataType.Text)]
        [RepeatNotations(5)]
        public string Sequence { get; set; }
        [Required]
        public string Structure { get; set; }
        [Required]
        [Display(Name = "Substrate Template")]
        public string SubstrateTemplate { get; set; }
        [Required]
        [Display(Name = "Substrate Structure")]
        public string SubstrateStructure { get; set; }
        [Display(Name = "Post Process")]
        public bool PostProcess { get; set; }

        public virtual Ribozyme Ribozyme { get; set; }
    }
}
