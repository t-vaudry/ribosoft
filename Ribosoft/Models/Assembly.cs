using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    public class Assembly : BaseEntity
    {
        [Key]
        [Display(Name = "Taxon")]
        public int TaxonomyId { get; set; }
        [Required]
        [Display(Name = "Accession ID")]
        public string AccessionId { get; set; }
        [Required]
        [Display(Name = "Species Taxon")]
        public int SpeciesId { get; set; }
        [Required]
        [Display(Name = "Organism")]
        public string OrganismName { get; set; }
        [Required]
        [Display(Name = "Assembly Name")]
        public string AssemblyName { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string Path { get; set; }
        [Required]
        [Display(Name = "Enabled?")]
        public bool IsEnabled { get; set; }

        public virtual ICollection<Job> Jobs { get; set; }

        public Assembly()
        {
            Jobs = new List<Job>();
        }
    }
}
