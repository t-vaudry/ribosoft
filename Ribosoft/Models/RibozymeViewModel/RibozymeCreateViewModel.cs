using System;
using System.ComponentModel.DataAnnotations;

namespace Ribosoft.Models.RibozymeViewModel
{
    [ValidateRibozymeStructure]
    public class RibozymeCreateViewModel
    {
        [Required]
        [Display(Name = "Ribozyme Name")]
        public string Name { get; set; }

        [Required]
        [Range(0, 30000)]
        public int Cutsite { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RepeatNotations(15)]
        [Nucleotide]
        public string Sequence { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RegularExpression(@"^[.()\[\]a-z0-9]+$", ErrorMessage = "Sequence structure must only contain ., (, ), [, ], letters, or numbers")]
        [UniqueAlphaNumericStructure]
        [ValidStructure]
        public string Structure { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [StringLength(200, MinimumLength = 1)]
        [Display(Name = "Substrate template")]
        [Nucleotide]
        public string SubstrateTemplate { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Display(Name = "Substrate structure")]
        [RegularExpression(@"^[\.a-z0-9]+$", ErrorMessage = "Subtrate structure must only contain ., letters, or numbers")]
        [UniqueAlphaNumericStructure]
        public string SubstrateStructure { get; set; }

        [Required]
        [Display(Name = "Post process?")]
        public bool PostProcess { get; set; }
    }
}