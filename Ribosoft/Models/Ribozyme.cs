using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    public class Ribozyme : BaseEntity
    {
        public Ribozyme()
        {
            this.RibozymeStructures = new HashSet<RibozymeStructure>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<RibozymeStructure> RibozymeStructures { get; set; }
    }
}
