using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Ribosoft.MultiObjectiveOptimization;

namespace Ribosoft.Models
{
    public class Design : BaseEntity, IRankable<OptimizeItem<float>>
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        [Display(Name = "Ribozyme Sequence")]
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

        [NotMapped]
        public virtual IEnumerable<OptimizeItem<float>> Comparables => new []
        {
            new OptimizeItem<float>(DesiredTemperatureScore.GetValueOrDefault(), OptimizeType.MIN, Job.DesiredTempTolerance.GetValueOrDefault()),
            new OptimizeItem<float>(HighestTemperatureScore.GetValueOrDefault(), OptimizeType.MAX, Job.HighestTempTolerance.GetValueOrDefault()),
            new OptimizeItem<float>(SpecificityScore.GetValueOrDefault(), OptimizeType.MIN, Job.SpecificityTolerance.GetValueOrDefault()),
            new OptimizeItem<float>(AccessibilityScore.GetValueOrDefault(), OptimizeType.MIN, Job.AccessibilityTolerance.GetValueOrDefault()),
            new OptimizeItem<float>(StructureScore.GetValueOrDefault(), OptimizeType.MIN, Job.StructureTolerance.GetValueOrDefault())
        };

        [NotMapped]
        [Display(Name = "Susbtrate Target Sequence")]
        public string SubstrateTargetSequence
        {
            get
            {
                return Job.RNAInput.Substring(CutsiteIndex, SubstrateSequenceLength);
            }
        }
    }
}
