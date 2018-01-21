using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models
{
    public class RequestViewModel
    {
        [Required]
        [Display(Name = "Ribozyme Structure:")]
        public int RibozymeStructure { get; set; }

        [Required]
        [RegularExpression(@"^['A','C','G','U','R','Y','K','M','S','W','B','D','H','V','N']+$", 
        ErrorMessage = "Sequence must only contain the following characters: A, C, G, U, R, Y, K, M, S, W, B, D, H, V, N")]
        [DataType(DataType.Text)]
        [Display(Name = "Input sequnce:")]
        public string InputSequence { get; set; }

        [Required]
        [Display(Name = "Select Target Region:")]
        public TargetRegion[] TargetRegions { get; set; }

        [Required]
        [Display(Name = "Environment:")]
        public TargetEnvironmentRadioInput TargetEnvironment { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "In-vivo Environment")]
        public string InVivoEnvironment { get; set; }

        [Required]
        [Display(Name = "Temperature (℃):")]
        public float Temperature { get; set; }

        [Required]
        [Display(Name = "Na (nM):")]
        public float Na { get; set; }

        [Required]
        [Display(Name = "Mg (nM):")]
        public float Mg { get; set; }

        [Required]
        [Display(Name = "Oligomer (nM):")]
        public float Oligomer { get; set; }

        [Display(Name = "Cut Sites:")]
        public string[] CutSites { get; set; }

        [Required]
        [Display(Name = "Method:")]
        public SpecificityRadioInput Specificity { get; set; }

    }

    public class TargetRegion
    {
        public TargetRegion()
        {
            this.Id = 0;
            this.Name = "";
            this.Selected = false;
        }
        public TargetRegion(int Id, string Name, bool Selected)
        {
            this.Id = Id;
            this.Name = Name;
            this.Selected = Selected;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }

    public class TargetEnvironmentRadioInput
    {
        public string TargetEnvironment { get; set; }
    }

    public class SpecificityRadioInput
    {
        public string Specificity { get; set; }
    }
}