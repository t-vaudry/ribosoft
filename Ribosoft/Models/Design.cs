﻿using System;
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
        public string Sequence { get; set; } = string.Empty;

        /*! \property IdealStructure
         * \brief Ribozyme ideal structure
         */
        [Display(Name = "Ribozyme Ideal Structure")]
        public string IdealStructure { get; set; } = string.Empty;

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

        /*! \property SubstrateSequence
         * \brief Substrate sequence
         */
        [Display(Name = "Substrate Sequence")]
        public string SubstrateSequence { get; set; } = string.Empty;

        /*! \property CutsiteIndex
         * \brief Index of the ribozyme cut-site (beginning of substrate sequence)
         */
        public int CutsiteIndex { get; set; }

        /*! \property SubstrateSequenceLength
         * \brief Length of the substrate sequence
         */
        public int SubstrateSequenceLength { get; set; }

        /*! \property Job
         * \brief Job object
         */
        public virtual Job Job { get; set; } = new Job();

        /*! \property Comparables
         * \brief List of optimize item comparables
         */
        [NotMapped]
        public virtual IEnumerable<OptimizeItem<float>> Comparables => new []
        {
            new OptimizeItem<float>(DesiredTemperatureScore.GetValueOrDefault(), OptimizeType.MIN, Job.DesiredTempTolerance.GetValueOrDefault()),
            new OptimizeItem<float>(SpecificityScore.GetValueOrDefault(), OptimizeType.MIN, Job.SpecificityTolerance.GetValueOrDefault()),
            new OptimizeItem<float>(AccessibilityScore.GetValueOrDefault(), OptimizeType.MIN, Job.AccessibilityTolerance.GetValueOrDefault()),
            new OptimizeItem<float>(StructureScore.GetValueOrDefault(), OptimizeType.MIN, Job.StructureTolerance.GetValueOrDefault())
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
