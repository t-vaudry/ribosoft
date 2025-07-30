using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCBI.Datasets.API.Models.Enums;

namespace NCBI.Datasets.API.Models.Requests
{
    /// <summary>
    /// Request model for organelle download operations
    /// </summary>
    public class OrganelleDownloadRequest
    {
        /// <summary>
        /// List of organelle accessions
        /// </summary>
        [Required]
        public List<string> Accessions { get; set; } = new List<string>();

        /// <summary>
        /// Set to true to omit the genomic sequence
        /// </summary>
        public bool? ExcludeSequence { get; set; }

        /// <summary>
        /// Annotation types to include in the download
        /// </summary>
        public List<AnnotationForOrganelleType> IncludeAnnotationType { get; set; } = new List<AnnotationForOrganelleType>();

        /// <summary>
        /// Hydration level for the dataset
        /// </summary>
        public HydrationLevel? Hydration { get; set; }
    }

    /// <summary>
    /// Request model for organelle metadata operations
    /// </summary>
    public class OrganelleMetadataRequest
    {
        /// <summary>
        /// List of taxon identifiers (can be tax IDs or names)
        /// </summary>
        public List<string> Taxons { get; set; } = new List<string>();

        /// <summary>
        /// List of organelle accessions
        /// </summary>
        public List<string> Accessions { get; set; } = new List<string>();

        /// <summary>
        /// Maximum number of results to return
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

        /// <summary>
        /// Returned content type
        /// </summary>
        public OrganelleMetadataRequestContentType? ReturnedContent { get; set; }
    }
}
