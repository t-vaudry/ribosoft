using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ribosoft.Models.RequestViewModels
{
    [ValidationAttributes.ValidateRequest]
    public class RequestViewModel
    {
        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "The Ribozyme structure field is required!")]
        [Display(Name = "Ribozyme structure")]
        public int RibozymeStructure { get; set; }

        [Required]
        [StringLength(30000, MinimumLength = 1)]
        [RegularExpression(@"^['A','C','G','T','U','R','Y','K','M','S','W','B','D','H','V','N']+$",
        ErrorMessage = "Sequence must only contain the following characters: A, C, G, T, U, R, Y, K, M, S, W, B, D, H, V, N")]
        [DataType(DataType.Text)]
        [Display(Name = "Input sequence")]
        public string InputSequence { get; set; }

        [Required]
        [ValidationAttributes.OpenReadingFrame]
        [Display(Name = "Open reading frame start index")]
        public int OpenReadingFrameStart { get; set; }

        [Required]
        [ValidationAttributes.OpenReadingFrame]
        [Display(Name = "Open reading frame end index")]
        public int OpenReadingFrameEnd { get; set; }

        [Required]
        [Display(Name = "Target regions")]
        public TargetRegionViewModel[] TargetRegions { get; set; }

        [Required]
        public TargetEnvironment SelectedTargetEnvironment { get; set; }

        [Display(Name = "Environment")]
        public IEnumerable<TargetEnvironmentViewModel> TargetEnvironments { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "In-vivo environment")]
        public int? InVivoEnvironment { get; set; }

        public SpecificityMethod SelectedSpecificityMethod { get; set; }

        [Display(Name = "Specificity method")]
        public IEnumerable<SpecificityMethodViewModel> SpecificityMethods { get; set; }

        [Required]
        [Range(-270.0f, 900.0f)]
        [Display(Name = "Temperature (℃)")]
        public float Temperature { get; set; }

        [Required]
        [Range(0.0f, 1000.0f)]
        [Display(Name = "Na⁺ (mM)")]
        public float Na { get; set; }

        [Required]
        [Range(0.01f, 10000.0f)]
        [Display(Name = "Probe (nM)")]
        public float Probe { get; set; }

        [Required]
        [Range(0.0f, 1.0f)]
        [Display(Name = "Desired Temperature Tolerance")]
        public float DesiredTemperatureTolerance { get; set; }

        [Required]
        [Range(0.0f, 1.0f)]
        [Display(Name = "Highest Temperature Tolerance")]
        public float HighestTemperatureTolerance { get; set; }

        [Required]
        [Range(0.0f, 1.0f)]
        [Display(Name = "Specificity Tolerance")]
        public float SpecificityTolerance { get; set; }

        [Required]
        [Range(0.0f, 1.0f)]
        [Display(Name = "Accessibility Tolerance")]
        public float AccessibilityTolerance { get; set; }

        [Required]
        [Range(0.0f, 1.0f)]
        [Display(Name = "Structure Tolerance")]
        public float StructureTolerance { get; set; }

        public int MaxRequests { get; set; }
        public bool ExceededMaxRequests { get; set; }

        public RequestViewModel()
        {
            Temperature = 37.0f;
            Na = 100.0f;
            Probe = 0.05f;
            MaxRequests = 20;
            ExceededMaxRequests = false;

            DesiredTemperatureTolerance = 0.05f;
            HighestTemperatureTolerance = 0.05f;
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
                new SpecificityMethodViewModel { Name = "Cleavage", Value = SpecificityMethod.CleavageOnly },
                new SpecificityMethodViewModel { Name = "Cleavage and hybridization", Value = SpecificityMethod.CleavageAndHybridization }
            };
        }
    }
}
