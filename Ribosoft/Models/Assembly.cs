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
        public int TaxonomyId { get; set; }
        [Required]
        public string AccessionId { get; set; }
        [Required]
        public int SpeciesId { get; set; }
        [Required]
        public string OrganismName { get; set; }
        [Required]
        public string AssemblyName { get; set; }
        [Required]        
        public string Type { get; set; }
        [Required]
        public string Path { get; set; }
        [Required]
        public bool IsEnabled { get; set; }

        public virtual ICollection<Job> Jobs { get; set; }

        public Assembly()
        {
            Jobs = new List<Job>();
        }
    }
}
