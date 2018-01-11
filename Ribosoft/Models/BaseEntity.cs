using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    public abstract class BaseEntity
    {
        [DisplayName("Created")]
        public DateTime? CreatedAt { get; set; }
        [DisplayName("Updated")]
        public DateTime? UpdatedAt { get; set; }
    }
}
