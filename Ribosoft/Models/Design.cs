using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    public class Design : BaseEntity
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string Sequence { get; set; }
        public int Rank { get; set; }
        [DisplayName("Temperature Score")]
        public float? TemperatureScore { get; set; }
        [DisplayName("Specialization Score")]
        public float? SpecializationScore { get; set; }
        [DisplayName("Accessibility Score")]
        public float? AccessibilityScore { get; set; }
        [DisplayName("Structure Score")]
        public float? StructureScore { get; set; }

        public virtual Job Job { get; set; }
    }
}
