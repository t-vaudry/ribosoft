using System;
using System.ComponentModel.DataAnnotations;

namespace Ribosoft.Models.RibozymeViewModel
{
    /*! \class RibozymeCreateViewModel
     * \brief Model class for the create ribozyme view
     */
    [ValidationAttributes.ValidateRibozymeStructure]
    public class RibozymeCreateViewModel
    {
        /*! \property Name
         * \brief Ribozyme name
         */
        [Required]
        [Display(Name = "Ribozyme Name")]
        public string Name { get; set; }

        /*! \property Cutsite
         * \brief Ribozyme cut-site
         */
        [Required]
        [Range(0, 30000)]
        public int Cutsite { get; set; }

        /*! \property Sequence
         * \brief Ribozyme template sequence
         */
        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [ValidationAttributes.RepeatNotations(50)]
        [ValidationAttributes.Nucleotide]
        public string Sequence { get; set; }

        /*! \property Structure
         * \brief Ribozyme template structure
         */
        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [RegularExpression(@"^[.()\[\]a-z0-9]+$", ErrorMessage = "Sequence structure must only contain ., (, ), [, ], letters, or numbers")]
        [ValidationAttributes.UniqueAlphaNumericStructure]
        [ValidationAttributes.ValidStructure]
        public string Structure { get; set; }

        /*! \property SubstrateTemplate
         * \brief Substrate template sequence
         */
        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [StringLength(200, MinimumLength = 1)]
        [Display(Name = "Substrate template")]
        [ValidationAttributes.Nucleotide]
        public string SubstrateTemplate { get; set; }

        /*! \property SubstrateStructure
         * \brief Substrate template structure
         */
        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Display(Name = "Substrate structure")]
        [RegularExpression(@"^[\.a-z0-9]+$", ErrorMessage = "Subtrate structure must only contain ., letters, or numbers")]
        [ValidationAttributes.UniqueAlphaNumericStructure]
        public string SubstrateStructure { get; set; }

        /*! \property PostProcess
         * \brief Currently unused
         */
        [Required]
        [Display(Name = "Post process?")]
        public bool PostProcess { get; set; }
    }
}