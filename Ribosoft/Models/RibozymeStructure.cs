using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    [ValidationAttributes.ValidateRibozymeStructure]
    public class RibozymeStructure : BaseEntity
    {
        public int Id { get; set; }
        public int RibozymeId { get; set; }
        [Required]
        public int Cutsite { get; set; }
        [Required]
        [DataType(DataType.Text)]
        [ValidationAttributes.RepeatNotations(50)]
        [ValidationAttributes.Nucleotide]
        public string Sequence { get; set; }
        [Required]
        [RegularExpression(@"^[.()\[\]a-z0-9]+$",
        ErrorMessage = "Sequence structure must only contain ., (, ), [, ], letters, or numbers")]
        [ValidationAttributes.UniqueAlphaNumericStructure]
        [ValidationAttributes.ValidStructure]
        public string Structure { get; set; }
        [Required]
        [Display(Name = "Substrate Template")]
        [ValidationAttributes.Nucleotide]
        public string SubstrateTemplate { get; set; }
        [Required]
        [Display(Name = "Substrate Structure")]
        [RegularExpression(@"^[\.a-z0-9]+$",
        ErrorMessage = "Subtrate structure must only contain ., letters, or numbers")]
        [ValidationAttributes.UniqueAlphaNumericStructure]
        public string SubstrateStructure { get; set; }
        [Required]
        [Display(Name = "Post Process")]
        public bool PostProcess { get; set; }

        public virtual Ribozyme Ribozyme { get; set; }
    }
}
