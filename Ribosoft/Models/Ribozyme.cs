using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    /*! \class Ribozyme
     * \brief Model class for the ribozyme containing its structures
     */
    public class Ribozyme : BaseEntity
    {
        /*! \fn Ribozyme
         * \brief Default constructor
         */
        public Ribozyme()
        {
            this.RibozymeStructures = new HashSet<RibozymeStructure>();
        }

        /*! \property Id
         * \brief Ribozyme ID
         */
        public int Id { get; set; }

        /*! \property Name
         * \brief Ribozyme name
         */
        [Required]
        public string Name { get; set; }

        /*! \property RibozymeStructures
         * \brief Collection of ribozyme structures
         */
        public virtual ICollection<RibozymeStructure> RibozymeStructures { get; set; }
    }
}
