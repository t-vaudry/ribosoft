using System;
using System.ComponentModel.DataAnnotations;

namespace Ribosoft.Models.RibozymeViewModel
{
    [ValidationAttributes.ValidateRibozymeStructure]
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
        [ValidationAttributes.RepeatNotations(50)]
        [ValidationAttributes.Nucleotide]
        private string sequence;
        public string Sequence
        { 
            get
            {
                return this.sequence;
            }
            set
            {
                this.sequence = value.Replace("t", "u").Replace("T", "U");
            }
        }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RegularExpression(@"^[.()\[\]a-z0-9]+$", ErrorMessage = "Sequence structure must only contain ., (, ), [, ], letters, or numbers")]
        [ValidationAttributes.UniqueAlphaNumericStructure]
        [ValidationAttributes.ValidStructure]
        public string Structure { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [StringLength(200, MinimumLength = 1)]
        [Display(Name = "Substrate template")]
        [ValidationAttributes.Nucleotide]
        private string substrateTemplate;
        public string SubstrateTemplate
        {
            get
            {
                return this.substrateTemplate;
            }
            set
            {
                this.substrateTemplate = value.Replace("t", "u").Replace("T", "U");
            }
        }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Display(Name = "Substrate structure")]
        [RegularExpression(@"^[\.a-z0-9]+$", ErrorMessage = "Subtrate structure must only contain ., letters, or numbers")]
        [ValidationAttributes.UniqueAlphaNumericStructure]
        public string SubstrateStructure { get; set; }

        [Required]
        [Display(Name = "Post process?")]
        public bool PostProcess { get; set; }
    }
}