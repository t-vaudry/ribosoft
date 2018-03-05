using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Ribosoft.MultiObjectiveOptimization;

namespace Ribosoft.Models
{
    public class Design : BaseEntity, IRankable<float>
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string Sequence { get; set; }
        public int Rank { get; set; }

        // fitness values
        [Display(Name = "Desired Temperature Score")]
        [DisplayFormat(DataFormatString="{0:0.###}")]
        public float? DesiredTemperatureScore { get; set; }
        [Display(Name = "Highest Temperature Score")]
        [DisplayFormat(DataFormatString="{0:0.###}")]
        public float? HighestTemperatureScore { get; set; }
        [Display(Name = "Specificity Score")]
        [DisplayFormat(DataFormatString="{0:0.###}")]
        public float? SpecificityScore { get; set; }
        [Display(Name = "Accessibility Score")]
        [DisplayFormat(DataFormatString="{0:0.###}")]
        public float? AccessibilityScore { get; set; }
        [Display(Name = "Structure Score")]
        [DisplayFormat(DataFormatString="{0:0.###}")]
        public float? StructureScore { get; set; }

        // substrate sequence
        public int CutsiteIndex { get; set; }
        public int SubstrateSequenceLength { get; set; }

        public virtual Job Job { get; set; }

        public virtual IEnumerable<float> Comparables => new []
        {
            DesiredTemperatureScore.GetValueOrDefault(),
            HighestTemperatureScore.GetValueOrDefault(),
            SpecificityScore.GetValueOrDefault(),
            AccessibilityScore.GetValueOrDefault(),
            StructureScore.GetValueOrDefault()
        };
    }
}
