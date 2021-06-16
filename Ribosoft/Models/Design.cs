using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Ribosoft.MultiObjectiveOptimization;

namespace Ribosoft.Models
{
    /*! \class Design
     * \brief Model class for the design of a ribozyme
     */
    public class Design : BaseEntity, IRankable<OptimizeItem<float>>
    {
        /*! \property Id
         * \brief Design ID
         */
        public int Id { get; set; }

        /*! \property JobId
         * \brief Job ID
         */
        public int JobId { get; set; }

        /*! \property Sequence
         * \brief Ribozyme sequence
         */
        [Display(Name = "Ribozyme Sequence")]
        public string Sequence { get; set; }

        /*! \property Structure
         * \brief Structure of the ribozyme
         */
        [Display(Name = "Ribozyme Structure")]
        public String Structure { get; set; }

        /*! \property Rank
         * \brief Ribozyme rank
         */
        [Display(Name = "Rank")]
        public int Rank { get; set; }

        // fitness values
        /*! \property DesiredTemperatureScore
         * \brief Desired Temperature Score
         */
        [Display(Name = "Desired Temperature Score")]
        [DisplayFormat(DataFormatString="{0:0.###}")]
        public float? DesiredTemperatureScore { get; set; }

        /*! \property HighestTemperatureScore
         * \brief Highest Temperature Score
         */
        [Display(Name = "Highest Temperature Score")]
        [DisplayFormat(DataFormatString="{0:0.###}")]
        public float? HighestTemperatureScore { get; set; }

        /*! \property SpecificityScore
         * \brief Specificity Score
         */
        [Display(Name = "Specificity Score")]
        [DisplayFormat(DataFormatString="{0:0.###}")]
        public float? SpecificityScore { get; set; }

        /*! \property AccessibilityScore
         * \brief Accessibility Score
         */
        [Display(Name = "Accessibility Score")]
        [DisplayFormat(DataFormatString="{0:0.###}")]
        public float? AccessibilityScore { get; set; }

        /*! \property StructureScore
         * \brief Structure Score
         */
        [Display(Name = "Structure Score")]
        [DisplayFormat(DataFormatString="{0:0.###}")]
        public float? StructureScore { get; set; }

        /*! \property MalformationScore
         * \brief Malformation Score
         */
        [Display(Name = "Malformation Score")]
        [DisplayFormat(DataFormatString = "{0:0.###}")]
        public float? MalformationScore { get; set; }

        /*! \property MalformationStructure
         * \brief MalformationStructure of the ribozyme
         */
        [Display(Name = "Malformation Ribozyme Structure")]
        public String MalformationStructure { get; set; }

        // substrate sequence
        /*! \property CutsiteIndex
         * \brief Index of the ribozyme cut-site
         */
        public int CutsiteIndex { get; set; }

        /*! \property SubstrateSequenceLength
         * \brief Length of the substrate sequence
         */
        public int SubstrateSequenceLength { get; set; }

        /*! \property Job
         * \brief Job object
         */
        public virtual Job Job { get; set; }

        /*! \property Comparables
         * \brief List of optimize item comparables
         */
        [NotMapped]
        public virtual IEnumerable<OptimizeItem<float>> Comparables => new []
        {
            new OptimizeItem<float>(DesiredTemperatureScore.GetValueOrDefault(), OptimizeType.MIN, Job.DesiredTempTolerance.GetValueOrDefault()),
            new OptimizeItem<float>(HighestTemperatureScore.GetValueOrDefault(), OptimizeType.MAX, Job.HighestTempTolerance.GetValueOrDefault()),
            new OptimizeItem<float>(SpecificityScore.GetValueOrDefault(), OptimizeType.MIN, Job.SpecificityTolerance.GetValueOrDefault()),
            new OptimizeItem<float>(AccessibilityScore.GetValueOrDefault(), OptimizeType.MIN, Job.AccessibilityTolerance.GetValueOrDefault()),
            new OptimizeItem<float>(StructureScore.GetValueOrDefault(), OptimizeType.MIN, Job.StructureTolerance.GetValueOrDefault()),
            new OptimizeItem<float>(MalformationScore.GetValueOrDefault(), OptimizeType.MAX, Job.MalformationTolerance.GetValueOrDefault())
        };

        /*! \property SubstrateTargetSequence
         * \brief Substrate Target Sequence
         */
        [NotMapped]
        [Display(Name = "Substrate Target Sequence")]
        public string SubstrateTargetSequence
        {
            get
            {
                return Job.RNAInput.Substring(CutsiteIndex, SubstrateSequenceLength);
            }
        }
    }
}
