using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    [ValidateRequest]
    public class RequestViewModel
    {
        [Required]
        [Display(Name = "Ribozyme structure")]
        public int RibozymeStructure { get; set; }

        [Required]
        [StringLength(30000, MinimumLength = 1)]
        [RegularExpression(@"^['A','C','G','U','R','Y','K','M','S','W','B','D','H','V','N']+$",
        ErrorMessage = "Sequence must only contain the following characters: A, C, G, U, R, Y, K, M, S, W, B, D, H, V, N")]
        [DataType(DataType.Text)]
        [Display(Name = "Input sequence")]
        public string InputSequence { get; set; }

        [Required]
        [OpenReadingFrame]
        [Display(Name = "Open reading frame start index")]
        public int OpenReadingFrameStart { get; set; }

        [Required]
        [OpenReadingFrame]
        [Display(Name = "Open reading frame end index")]
        public int OpenReadingFrameEnd { get; set; }

        [Required]
        [Display(Name = "Target regions")]
        public TargetRegion[] TargetRegions { get; set; }

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
        [Display(Name = "Na (nM)")]
        public float Na { get; set; }

        [Required]
        [Range(0.01f, 10000.0f)]
        [Display(Name = "Probe (nM)")]
        public float Probe { get; set; }

        // [Display(Name = "Cut Sites")]
        // public string[] CutSites { get; set; }

        public RequestViewModel()
        {
            Temperature = 0.0f;
            Na = 1.0f;
            Probe = 0.05f;

            TargetRegions = new[]
            {
                new TargetRegion { Id = 1, Name = "5'UTR", Selected = true },
                new TargetRegion { Id = 2, Name = "Open reading frame (ORF)", Selected = true },
                new TargetRegion { Id = 3, Name = "3'UTR", Selected = true }
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

    public class TargetRegion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }

    public class TargetEnvironmentViewModel
    {
        public string Name { get; set; }
        public TargetEnvironment Value { get; set; }
    }

    public class SpecificityMethodViewModel
    {
        public string Name { get; set; }
        public SpecificityMethod Value { get; set; }
    }
}
