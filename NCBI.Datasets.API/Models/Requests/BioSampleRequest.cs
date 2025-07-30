using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCBI.Datasets.API.Models.Requests
{
    /// <summary>
    /// Request model for BioSample dataset report operations
    /// </summary>
    public class BioSampleDatasetReportRequest
    {
        /// <summary>
        /// List of BioSample accessions
        /// </summary>
        [Required]
        public List<string> Accessions { get; set; } = new List<string>();

        /// <summary>
        /// Maximum number of results to return (default: 20, max: 1000)
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Page token for pagination
        /// </summary>
        public string? PageToken { get; set; }

        /// <summary>
        /// Whether to include tabular header row
        /// </summary>
        public bool? IncludeTabularHeader { get; set; }
    }
}
