using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

//! \namespace Ribosoft.Models.RequestViewModels
namespace Ribosoft.Models.RequestViewModels
{
    /*! \class RequestViewModel
     * \brief This model is used to format the main request of the system.
     */
    [ValidationAttributes.ValidateRequest]
    public class RequestViewModel
    {
        /*! \property RibozymeStructure
         * \brief Ribozyme structure
         */
        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "The Ribozyme structure field is required!")]
        [Display(Name = "Ribozyme structure")]
        public int RibozymeStructure { get; set; }

        /*! \property InputSequence
         * \brief Input RNA sequence
         */
        [Required]
        [StringLength(30000, MinimumLength = 1)]
        [RegularExpression(@"^['A','C','G','T','U','R','Y','K','M','S','W','B','D','H','V','N']+$",
        ErrorMessage = "Sequence must only contain the following characters: A, C, G, T, U, R, Y, K, M, S, W, B, D, H, V, N")]
        [DataType(DataType.Text)]
        [Display(Name = "Input sequence")]
        public string InputSequence 
        { 
            get
            {
                return this.inputSequence;
            }
            set
            {
                this.inputSequence = value.Replace("\n", "").Replace("\r", "").ToUpper();
            }
        }
        /*! \property inputSequence
         * \brief Input RNA sequence
         */
        private string inputSequence;

        /*! \property OpenReadingFrameStart
         * \brief Index of the start of the open reading frame
         */
        [Required]
        [ValidationAttributes.OpenReadingFrame]
        [Display(Name = "Open reading frame start index")]
        public int OpenReadingFrameStart { get; set; }

        /*! \property OpenReadingFrameEnd
         * \brief Index of the end of the open reading frame
         */
        [Required]
        [ValidationAttributes.OpenReadingFrame]
        [Display(Name = "Open reading frame end index")]
        public int OpenReadingFrameEnd { get; set; }

        /*! \property TargetRegions
         * \brief Array of target regions to cover in request
         */
        [Required]
        [Display(Name = "Target regions")]
        public TargetRegionViewModel[] TargetRegions { get; set; }

        /*! \property TargetTemperature
         * \brief Target temperature of request
         */
        [Required]
        [Range(-270.0f, 900.0f)]
        [Display(Name = "Target Temperature (℃)")]
        public float TargetTemperature { get; set; }

        /*! \property SelectedTargetEnvironment
         * \brief Selected target environment
         */
        [Required]
        public TargetEnvironment SelectedTargetEnvironment { get; set; }

        /*! \property TargetEnvironments
         * \brief List of available target environments
         */
        [Display(Name = "Environment")]
        public IEnumerable<TargetEnvironmentViewModel> TargetEnvironments { get; set; }

        /*! \property InVivoEnvironment
         * \brief In-vivo environment
         */
        [DataType(DataType.Text)]
        [Display(Name = "In-vivo environment")]
        public int? InVivoEnvironment { get; set; }

        /*! \property SelectedSpecificityMethod
         * \brief Selected specificity method
         */
        public SpecificityMethod SelectedSpecificityMethod { get; set; }

        /*! \property SpecificityMethods
         * \brief List of available specificity methods
         */
        [Display(Name = "Specificity method")]
        public IEnumerable<SpecificityMethodViewModel> SpecificityMethods { get; set; }

        /*! \property Temperature
         * \brief Temperature of request
         */
        [Required]
        [Range(-270.0f, 900.0f)]
        [Display(Name = "Temperature (℃)")]
        public float Temperature { get; set; }

        /*! \property Na
         * \brief Na⁺ (mM) concentration
         */
        [Required]
        [Range(0.0f, 1000.0f)]
        [Display(Name = "Na⁺ (mM)")]
        public float Na { get; set; }

        /*! \property Probe
         * \brief Probe (nM) concentration
         */
        [Required]
        [Range(0.01f, 10000.0f)]
        [Display(Name = "Probe (nM)")]
        public float Probe { get; set; }

        /*! \property DesiredTemperatureTolerance
         * \brief Tolerance for desired temperature
         */
        [Required]
        [Range(0.0f, 1.0f)]
        [Display(Name = "Desired Temperature Tolerance")]
        public float DesiredTemperatureTolerance { get; set; }

        /*! \property SpecificityTolerance
         * \brief Tolerance for specifity
         */
        [Required]
        [Range(0.0f, 1.0f)]
        [Display(Name = "Specificity Tolerance")]
        public float SpecificityTolerance { get; set; }

        /*! \property AccessibilityTolerance
         * \brief Tolerance for accessibility
         */
        [Required]
        [Range(0.0f, 1.0f)]
        [Display(Name = "Accessibility Tolerance")]
        public float AccessibilityTolerance { get; set; }

        /*! \property StructureTolerance
         * \brief Tolerance for structure
         */
        [Required]
        [Range(0.0f, 1.0f)]
        [Display(Name = "Structure Tolerance")]
        public float StructureTolerance { get; set; }

        /*! \property MaxRequests
         * \brief Maximum number of requests per user
         */
        public int MaxRequests { get; set; }

        /*! \property ExceededMaxRequests
         * \brief Check if user exceeds maximum requests
         */
        public bool ExceededMaxRequests { get; set; }

        /*! \fn RequestViewModel
         * \brief Default constructor
         */
        public RequestViewModel()
        {
            TargetTemperature = 22.0f;
            Temperature = 37.0f;
            Na = 100.0f;
            Probe = 0.05f;
            MaxRequests = 20;
            ExceededMaxRequests = false;

            DesiredTemperatureTolerance = 0.05f;
            SpecificityTolerance = 0.05f;
            AccessibilityTolerance = 0.05f;
            StructureTolerance = 0.05f;

            TargetRegions = new[]
            {
                new TargetRegionViewModel { Id = 1, Name = "5'UTR", Selected = true },
                new TargetRegionViewModel { Id = 2, Name = "Open reading frame (ORF)", Selected = true },
                new TargetRegionViewModel { Id = 3, Name = "3'UTR", Selected = true }
            };

            TargetEnvironments = new List<TargetEnvironmentViewModel>
            {
                new TargetEnvironmentViewModel { Name = "In-vitro", Value = TargetEnvironment.InVitro },
                new TargetEnvironmentViewModel { Name = "In-vivo", Value = TargetEnvironment.InVivo }
            };

            SpecificityMethods = new List<SpecificityMethodViewModel>
            {
                new SpecificityMethodViewModel { Name = "Synthetic", Value = SpecificityMethod.Synthetic },
                new SpecificityMethodViewModel { Name = "Wildtype", Value = SpecificityMethod.Wildtype }
            };
        }
    }
}
