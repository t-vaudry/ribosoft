using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Display(Name = "Temperature Score")]
        public float? TemperatureScore { get; set; }
        [Display(Name = "Specificity Score")]
        public float? SpecificityScore { get; set; }
        [Display(Name = "Accessibility Score")]
        public float? AccessibilityScore { get; set; }
        [Display(Name = "Structure Score")]
        public float? StructureScore { get; set; }

        public virtual Job Job { get; set; }
    }
}
