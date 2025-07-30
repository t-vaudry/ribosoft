using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Enums;

namespace NCBI.Datasets.API.Models.Requests
{
    /// <summary>
    /// Request model for taxonomy dataset operations
    /// </summary>
    public class TaxonomyDatasetRequest
    {
        /// <summary>
        /// List of taxonomy IDs to retrieve
        /// </summary>
        public List<int> TaxIds { get; set; } = new List<int>();

        /// <summary>
        /// Additional reports to include in the response
        /// </summary>
        public List<TaxonomyDatasetRequestTaxonomyReportType> AuxReports { get; set; } = new List<TaxonomyDatasetRequestTaxonomyReportType>();
    }

    /// <summary>
    /// Request model for taxonomy metadata operations
    /// </summary>
    public class TaxonomyMetadataRequest
    {
        /// <summary>
        /// List of taxon identifiers (can be tax IDs or names)
        /// </summary>
        [Required]
        public List<string> Taxons { get; set; } = new List<string>();

        /// <summary>
        /// Type of content to return
        /// </summary>
        public TaxonomyMetadataRequestContentType ReturnedContent { get; set; } = TaxonomyMetadataRequestContentType.COMPLETE;

        /// <summary>
        /// Maximum number of results to return (default: 20, max: 1000)
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Whether to include tabular header row
        /// </summary>
        public bool? IncludeTabularHeader { get; set; }

        /// <summary>
        /// Page token for pagination
        /// </summary>
        public string? PageToken { get; set; }

        /// <summary>
        /// Format for tabular data
        /// </summary>
        public TaxonomyMetadataRequestTableFormat? TableFormat { get; set; }

        /// <summary>
        /// Flag for taxonomy explosion (include children)
        /// </summary>
        public bool? Children { get; set; }

        /// <summary>
        /// Taxonomic ranks to filter by
        /// </summary>
        public List<RankType> Ranks { get; set; } = new List<RankType>();
    }

    /// <summary>
    /// Request model for taxonomy filtered subtree operations
    /// </summary>
    public class TaxonomyFilteredSubtreeRequest
    {
        /// <summary>
        /// List of taxon identifiers
        /// </summary>
        [Required]
        public List<string> Taxons { get; set; } = new List<string>();

        /// <summary>
        /// Limit to specified species only
        /// </summary>
        public bool? SpecifiedLimit { get; set; }

        /// <summary>
        /// Rank limits for filtering
        /// </summary>
        public List<RankType> RankLimits { get; set; } = new List<RankType>();

        /// <summary>
        /// Maximum number of children to return
        /// </summary>
        public int? ChildrenLimit { get; set; }

        /// <summary>
        /// Maximum depth for subtree traversal
        /// </summary>
        public int? DepthLimit { get; set; }
    }

    /// <summary>
    /// Request model for taxonomy related IDs operations
    /// </summary>
    public class TaxonomyRelatedIdRequest
    {
        /// <summary>
        /// Taxonomy ID to find related IDs for
        /// </summary>
        [Required]
        public int TaxId { get; set; }

        /// <summary>
        /// Types of relationships to include
        /// </summary>
        public List<TaxonomyRelationshipType> RelationshipTypes { get; set; } = new List<TaxonomyRelationshipType>();
    }

    /// <summary>
    /// Request model for taxonomy suggestion operations
    /// </summary>
    public class TaxonomySuggestionRequest
    {
        /// <summary>
        /// Partial taxonomic name to search for
        /// </summary>
        [Required]
        public string TaxonQuery { get; set; } = string.Empty;

        /// <summary>
        /// Maximum number of suggestions to return
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Taxonomic ranks to filter suggestions
        /// </summary>
        public List<RankType> Ranks { get; set; } = new List<RankType>();
    }

    /// <summary>
    /// Request model for taxonomy links operations
    /// </summary>
    public class TaxonomyLinksRequest
    {
        /// <summary>
        /// Taxon identifier to get links for
        /// </summary>
        [Required]
        public string Taxon { get; set; } = string.Empty;

        /// <summary>
        /// Types of links to include
        /// </summary>
        public List<TaxonomyLinkType> LinkTypes { get; set; } = new List<TaxonomyLinkType>();
    }

    /// <summary>
    /// Request model for taxonomy image operations
    /// </summary>
    public class TaxonomyImageRequest
    {
        /// <summary>
        /// Taxon identifier to get image for
        /// </summary>
        [Required]
        public string Taxon { get; set; } = string.Empty;

        /// <summary>
        /// Image size preference
        /// </summary>
        public TaxonomyImageSize? ImageSize { get; set; }

        /// <summary>
        /// Image format preference
        /// </summary>
        public TaxonomyImageFormat? ImageFormat { get; set; }
    }

    /// <summary>
    /// Request model for taxonomy image metadata operations
    /// </summary>
    public class TaxonomyImageMetadataRequest
    {
        /// <summary>
        /// Taxon identifier to get image metadata for
        /// </summary>
        [Required]
        public string Taxon { get; set; } = string.Empty;

        /// <summary>
        /// Include detailed metadata
        /// </summary>
        public bool? IncludeDetails { get; set; }
    }
}
