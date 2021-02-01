using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    /*! \class RibozymeStructure
     * \brief Model class for a ribozyme structure
     */
    [ValidationAttributes.ValidateRibozymeStructure]
    public class RibozymeStructure : BaseEntity
    {
        /*! \property Id
         * \brief Ribozyme structure ID
         */
        public int Id { get; set; }

        /*! \property RibozymeId
         * \brief Ribozyme ID
         */
        public int RibozymeId { get; set; }

        /*! \property Cutsite
         * \brief Ribozyme cut-site
         */
        [Required]
        public int Cutsite { get; set; }

        /*! \property Sequence
         * \brief Ribozyme sequence template
         */
        [Required]
        [DataType(DataType.Text)]
        [ValidationAttributes.RepeatNotations(50)]
        [ValidationAttributes.Nucleotide]
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
        /*! \property sequence
         * \brief Ribozyme sequence
         */
        private string sequence;

        /*! \property Structure
         * \brief Ribozyme structure template
         */
        [Required]
        [RegularExpression(@"^[.()\[\]a-z0-9]+$",
        ErrorMessage = "Sequence structure must only contain ., (, ), [, ], letters, or numbers")]
        [ValidationAttributes.UniqueAlphaNumericStructure]
        [ValidationAttributes.ValidStructure]
        public string Structure { get; set; }

        /*! \property SubstrateTemplate
         * \brief Substrate sequence template
         */
        [Required]
        [Display(Name = "Substrate Template")]
        [ValidationAttributes.Nucleotide]
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
        /*! \property substrateTemplate
         * \brief Substrate sequence template
         */
        private string substrateTemplate;

        /*! \property SubstrateStructure
         * \brief Substrate structure template
         */
        [Required]
        [Display(Name = "Substrate Structure")]
        [RegularExpression(@"^[\.a-z0-9]+$",
        ErrorMessage = "Subtrate structure must only contain ., letters, or numbers")]
        [ValidationAttributes.UniqueAlphaNumericStructure]
        public string SubstrateStructure { get; set; }

        /*! \property PostProcess
         * \brief Currently unused
         */
        [Required]
        [Display(Name = "Post Process")]
        public bool PostProcess { get; set; }

        /*! \property Ribozyme
         * \brief Ribozyme object
         */
        public virtual Ribozyme Ribozyme { get; set; }
    }
}
