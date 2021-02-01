using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

//! \namespace Ribosoft.Models
namespace Ribosoft.Models
{
    /*! \class Assembly
     * \brief This model is used for the Assembly databases that are accessible to the system.
     */
    public class Assembly : BaseEntity
    {
        /*! \property TaxonomyId
         * \brief Taxon
         */
        [Key]
        [Display(Name = "Taxon")]
        public int TaxonomyId { get; set; }

        /*! \property AccessionId
         * \brief Accession ID
         */
        [Required]
        [Display(Name = "Accession ID")]
        public string AccessionId { get; set; }

        /*! \property SpeciesId
         * \brief Species Taxon
         */
        [Required]
        [Display(Name = "Species Taxon")]
        public int SpeciesId { get; set; }

        /*! \property OrganismName
         * \brief Organism
         */
        [Required]
        [Display(Name = "Organism")]
        public string OrganismName { get; set; }

        /*! \property AssemblyName
         * \brief Assembly Name
         */
        [Required]
        [Display(Name = "Assembly Name")]
        public string AssemblyName { get; set; }

        /*! \property Type
         * \brief Assembly type
         */
        [Required]
        public string Type { get; set; }

        /*! \property Path
         * \brief Assembly database path
         */
        [Required]
        public string Path { get; set; }

        /*! \property IsEnabled
         * \brief Check if assembly is enabled
         */
        [Required]
        [Display(Name = "Enabled?")]
        public bool IsEnabled { get; set; }

        /*! \property Jobs
         * \brief Collection of jobs
         */
        public virtual ICollection<Job> Jobs { get; set; }

        /*! \fn Assembly
         * \brief Default constructor
         */
        public Assembly()
        {
            Jobs = new List<Job>();
        }
    }
}
