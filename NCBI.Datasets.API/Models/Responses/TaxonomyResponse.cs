using System.Collections.Generic;
using System.Text.Json.Serialization;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Enums;

namespace NCBI.Datasets.API.Models.Responses
{
    /// <summary>
    /// Response model for taxonomy metadata operations
    /// </summary>
    public class TaxonomyMetadataResponse
    {
        /// <summary>
        /// Messages from the API response
        /// </summary>
        public List<ReportsMessage> Messages { get; set; } = new List<ReportsMessage>();

        /// <summary>
        /// List of taxonomy nodes matching the request
        /// </summary>
        public List<TaxonomyMatch> TaxonomyNodes { get; set; } = new List<TaxonomyMatch>();

        /// <summary>
        /// Next page token for pagination
        /// </summary>
        public string? NextPageToken { get; set; }

        /// <summary>
        /// Total count of available results
        /// </summary>
        public int? TotalCount { get; set; }
    }

    /// <summary>
    /// Represents a taxonomy match result
    /// </summary>
    public class TaxonomyMatch
    {
        /// <summary>
        /// Warnings associated with this match
        /// </summary>
        public List<ReportsWarning> Warnings { get; set; } = new List<ReportsWarning>();

        /// <summary>
        /// Errors associated with this match
        /// </summary>
        public List<ReportsError> Errors { get; set; } = new List<ReportsError>();

        /// <summary>
        /// Query terms that matched this result
        /// </summary>
        public List<string> Query { get; set; } = new List<string>();

        /// <summary>
        /// The taxonomy node information
        /// </summary>
        public TaxonomyNode? Taxonomy { get; set; }
    }

    /// <summary>
    /// Represents a taxonomy node with detailed information
    /// </summary>
    public class TaxonomyNode
    {
        /// <summary>
        /// NCBI Taxonomy identifier
        /// </summary>
        public int TaxId { get; set; }

        /// <summary>
        /// Scientific name of the organism
        /// </summary>
        public string? OrganismName { get; set; }

        /// <summary>
        /// Common name of the organism
        /// </summary>
        public string? CommonName { get; set; }

        /// <summary>
        /// Parent taxonomy ID
        /// </summary>
        public int? ParentTaxId { get; set; }

        /// <summary>
        /// Taxonomic rank
        /// </summary>
        public RankType Rank { get; set; }

        /// <summary>
        /// Whether the taxonomy node has a proper species name
        /// </summary>
        public bool? HasDescribedSpeciesName { get; set; }

        /// <summary>
        /// Counts by different data types
        /// </summary>
        public List<TaxonomyNodeCountByType> Counts { get; set; } = new List<TaxonomyNodeCountByType>();

        /// <summary>
        /// Minimum ordinal value for range-based lookups
        /// </summary>
        public int? MinOrd { get; set; }

        /// <summary>
        /// Maximum ordinal value for range-based lookups
        /// </summary>
        public int? MaxOrd { get; set; }

        /// <summary>
        /// Whether the organism is extinct
        /// </summary>
        public bool? Extinct { get; set; }

        /// <summary>
        /// Genomic molecule type (dsDNA, ssDNA, ssRNA, etc.)
        /// </summary>
        public string? GenomicMoltype { get; set; }

        /// <summary>
        /// Full taxonomic lineage
        /// </summary>
        public List<TaxonomyNode> Lineage { get; set; } = new List<TaxonomyNode>();

        /// <summary>
        /// Child taxonomy nodes
        /// </summary>
        public List<TaxonomyNode> Children { get; set; } = new List<TaxonomyNode>();
    }

    /// <summary>
    /// Count information by data type for a taxonomy node
    /// </summary>
    public class TaxonomyNodeCountByType
    {
        /// <summary>
        /// Type of count (genomes, proteins, etc.)
        /// </summary>
        public CountType Type { get; set; }

        /// <summary>
        /// Count value
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// Response model for taxonomy filtered subtree operations
    /// </summary>
    public class TaxonomyFilteredSubtreeResponse
    {
        /// <summary>
        /// Messages from the API response
        /// </summary>
        public List<ReportsMessage> Messages { get; set; } = new List<ReportsMessage>();

        /// <summary>
        /// Root nodes of the filtered subtree
        /// </summary>
        public List<TaxonomyNode> Nodes { get; set; } = new List<TaxonomyNode>();

        /// <summary>
        /// Total number of nodes in the subtree
        /// </summary>
        public int? TotalNodes { get; set; }

        /// <summary>
        /// Whether the subtree was truncated due to limits
        /// </summary>
        public bool? Truncated { get; set; }
    }

    /// <summary>
    /// Response model for taxonomy suggestion operations
    /// </summary>
    public class TaxonomySuggestionResponse
    {
        /// <summary>
        /// List of taxonomy suggestions (matches API field name)
        /// </summary>
        [JsonPropertyName("sci_name_and_ids")]
        public List<TaxonomySuggestion> Suggestions { get; set; } = new List<TaxonomySuggestion>();

        /// <summary>
        /// Total number of available suggestions
        /// </summary>
        public int? TotalCount { get; set; }
    }

    /// <summary>
    /// Represents a taxonomy suggestion
    /// </summary>
    public class TaxonomySuggestion
    {
        /// <summary>
        /// Taxonomy ID as string (matches API field name)
        /// </summary>
        [JsonPropertyName("tax_id")]
        public string TaxIdString { get; set; } = string.Empty;

        /// <summary>
        /// Taxonomy ID as integer
        /// </summary>
        [JsonIgnore]
        public int TaxId => int.TryParse(TaxIdString, out var id) ? id : 0;

        /// <summary>
        /// Scientific name (matches API field name)
        /// </summary>
        [JsonPropertyName("sci_name")]
        public string? ScientificName { get; set; }

        /// <summary>
        /// Common name (matches API field name)
        /// </summary>
        [JsonPropertyName("common_name")]
        public string? CommonName { get; set; }

        /// <summary>
        /// Matched term (matches API field name)
        /// </summary>
        [JsonPropertyName("matched_term")]
        public string? MatchedTerm { get; set; }

        /// <summary>
        /// Taxonomic rank (matches API field name)
        /// </summary>
        [JsonPropertyName("rank")]
        public string? Rank { get; set; }

        /// <summary>
        /// Match score for the suggestion
        /// </summary>
        [JsonIgnore]
        public double? Score { get; set; }

        /// <summary>
        /// Highlighted portions of the match
        /// </summary>
        [JsonIgnore]
        public List<string> Highlights { get; set; } = new List<string>();
    }

    /// <summary>
    /// Response model for taxonomy related IDs operations
    /// </summary>
    public class TaxonomyRelatedIdResponse
    {
        /// <summary>
        /// Messages from the API response
        /// </summary>
        public List<ReportsMessage> Messages { get; set; } = new List<ReportsMessage>();

        /// <summary>
        /// Related taxonomy IDs grouped by relationship type
        /// </summary>
        public Dictionary<TaxonomyRelationshipType, List<int>> RelatedIds { get; set; } = new Dictionary<TaxonomyRelationshipType, List<int>>();

        /// <summary>
        /// Source taxonomy ID
        /// </summary>
        public int SourceTaxId { get; set; }
    }

    /// <summary>
    /// Response model for taxonomy links operations
    /// </summary>
    public class TaxonomyLinksResponse
    {
        /// <summary>
        /// Messages from the API response
        /// </summary>
        public List<ReportsMessage> Messages { get; set; } = new List<ReportsMessage>();

        /// <summary>
        /// List of external links for the taxonomy
        /// </summary>
        public List<TaxonomyLinksResponseGenericLink> Links { get; set; } = new List<TaxonomyLinksResponseGenericLink>();

        /// <summary>
        /// Source taxon identifier
        /// </summary>
        public string? SourceTaxon { get; set; }
    }

    /// <summary>
    /// Represents a generic external link for taxonomy
    /// </summary>
    public class TaxonomyLinksResponseGenericLink
    {
        /// <summary>
        /// Display name for the link
        /// </summary>
        public string? LinkName { get; set; }

        /// <summary>
        /// URL for the external link
        /// </summary>
        public string? LinkUrl { get; set; }

        /// <summary>
        /// Type of link
        /// </summary>
        public TaxonomyLinkType LinkType { get; set; }

        /// <summary>
        /// Description of the link
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Response model for taxonomy image operations
    /// </summary>
    public class TaxonomyImageResponse
    {
        /// <summary>
        /// Binary image data
        /// </summary>
        public byte[]? ImageData { get; set; }

        /// <summary>
        /// Content type of the image
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Image metadata
        /// </summary>
        public TaxonomyImageMetadata? Metadata { get; set; }
    }

    /// <summary>
    /// Response model for taxonomy image metadata operations
    /// </summary>
    public class TaxonomyImageMetadataResponse
    {
        /// <summary>
        /// Messages from the API response
        /// </summary>
        public List<ReportsMessage> Messages { get; set; } = new List<ReportsMessage>();

        /// <summary>
        /// Image metadata information
        /// </summary>
        public TaxonomyImageMetadata? Metadata { get; set; }

        /// <summary>
        /// Source taxon identifier
        /// </summary>
        public string? SourceTaxon { get; set; }
    }

    /// <summary>
    /// Metadata information for taxonomy images
    /// </summary>
    public class TaxonomyImageMetadata
    {
        /// <summary>
        /// Image width in pixels
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Image height in pixels
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// Image format (JPEG, PNG, etc.)
        /// </summary>
        public string? Format { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// Image source or attribution
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Copyright information
        /// </summary>
        public string? Copyright { get; set; }

        /// <summary>
        /// Image description or caption
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Date the image was created or last modified
        /// </summary>
        public string? LastModified { get; set; }
    }
}
