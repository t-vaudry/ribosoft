using System.ComponentModel.DataAnnotations;

namespace Ribosoft.Models.RibozymeViewModel
{
    public class RibozymeCreateViewModel
    {
        [Required]
        [Display(Name = "Ribozyme Name")]
        public string Name { get; set; }

        [Required]
        public int Cutsite { get; set; }

        [Required(AllowEmptyStrings=false)]
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        public string Sequence { get; set; }

        [Required(AllowEmptyStrings=false)]
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        public string Structure { get; set; }

        [Required(AllowEmptyStrings=false)]
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        [Display(Name = "Substrate template")]
        public string SubstrateTemplate { get; set; }

        [Required(AllowEmptyStrings=false)]
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        [Display(Name = "Substrate structure")]
        public string SubstrateStructure { get; set; }

        [Required]
        [Display(Name = "Post process?")]
        public bool PostProcess { get; set; }
    }
}