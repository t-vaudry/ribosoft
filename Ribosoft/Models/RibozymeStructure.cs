using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    public class RibozymeStructure : BaseEntity
    {
        public int Id { get; set; }
        public int RibozymeId { get; set; }
        public int Cutsite { get; set; }
        public string Sequence { get; set; }
        public string Structure { get; set; }
        [DisplayName("Substrate Template")]
        public string SubstrateTemplate { get; set; }
        [DisplayName("Substrate Structure")]
        public string SubstrateStructure { get; set; }
        [DisplayName("Post Process")]
        public bool PostProcess { get; set; }

        public virtual Ribozyme Ribozyme { get; set; }
    }
}
