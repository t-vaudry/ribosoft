using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
	/*! \class BaseEntity
     * \brief Model class for the base entity of every model
     */
    public abstract class BaseEntity
    {
        /*! \property CreatedAt
         * \brief Create date/time
         */
        [DisplayName("Created")]
        public DateTime? CreatedAt { get; set; }

        /*! \property UpdatedAt
         * \brief Update date/time
         */
        [DisplayName("Updated")]
        public DateTime? UpdatedAt { get; set; }
    }
}
