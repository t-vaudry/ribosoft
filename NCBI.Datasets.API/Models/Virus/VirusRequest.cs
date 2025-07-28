using NCBI.Datasets.API.Models.Enums;
using System.Text.Json.Serialization;

namespace NCBI.Datasets.API.Models.Virus;

/// <summary>
/// Request for virus dataset operations
/// </summary>
public class VirusDatasetRequest
{
    /// <summary>
    /// Virus genome accessions
    /// </summary>
    [JsonPropertyName("accessions")]
    public List<string>? Accessions { get; set; }

    /// <summary>
    /// NCBI Taxonomy ID or name (common or scientific) at any taxonomic rank
    /// </summary>
    [JsonPropertyName("taxon")]
    public string? Taxon { get; set; }

    /// <summary>
    /// Multiple taxonomy IDs or names
    /// </summary>
    [JsonPropertyName("taxons")]
    public List<string>? Taxons { get; set; }

    /// <summary>
    /// Limit to RefSeq genomes only
    /// </summary>
    [JsonPropertyName("refseq_only")]
    public bool? RefSeqOnly { get; set; }

    /// <summary>
    /// Virus dataset filter
    /// </summary>
    [JsonPropertyName("filter")]
    public VirusDatasetFilter? Filter { get; set; }

    /// <summary>
    /// Output filename
    /// </summary>
    [JsonPropertyName("filename")]
    public string? Filename { get; set; }

    /// <summary>
    /// Include annotation types
    /// </summary>
    [JsonPropertyName("include_annotation_type")]
    public List<VirusAnnotationType>? IncludeAnnotationType { get; set; }
}

/// <summary>
/// Request for virus data reports
/// </summary>
public class VirusDataReportRequest
{
    /// <summary>
    /// All the supported filters for virus requests
    /// </summary>
    [JsonPropertyName("filter")]
    public VirusDatasetFilter? Filter { get; set; }

    /// <summary>
    /// Return either virus genome accessions, or complete virus metadata
    /// </summary>
    [JsonPropertyName("returned_content")]
    public VirusDataReportRequestContentType? ReturnedContent { get; set; }

    /// <summary>
    /// Specify which fields to include in the tabular report
    /// </summary>
    [JsonPropertyName("table_fields")]
    public List<string>? TableFields { get; set; }

    /// <summary>
    /// Optional pre-defined template for processing a tabular data request
    /// </summary>
    [JsonPropertyName("table_format")]
    public string? TableFormat { get; set; }

    /// <summary>
    /// The maximum number of virus data reports to return
    /// </summary>
    [JsonPropertyName("page_size")]
    public int? PageSize { get; set; }

    /// <summary>
    /// Page token for pagination
    /// </summary>
    [JsonPropertyName("page_token")]
    public string? PageToken { get; set; }
}

/// <summary>
/// Virus dataset filter
/// </summary>
public class VirusDatasetFilter
{
    /// <summary>
    /// Virus genome accessions
    /// </summary>
    [JsonPropertyName("accessions")]
    public List<string>? Accessions { get; set; }

    /// <summary>
    /// NCBI Taxonomy ID or name
    /// </summary>
    [JsonPropertyName("taxon")]
    public string? Taxon { get; set; }

    /// <summary>
    /// Multiple taxonomy IDs or names
    /// </summary>
    [JsonPropertyName("taxons")]
    public List<string>? Taxons { get; set; }

    /// <summary>
    /// Limit to RefSeq genomes only
    /// </summary>
    [JsonPropertyName("refseq_only")]
    public bool? RefSeqOnly { get; set; }

    /// <summary>
    /// Limit results to genomes extracted from this host
    /// </summary>
    [JsonPropertyName("host")]
    public string? Host { get; set; }

    /// <summary>
    /// Limit results to genomes classified to this lineage by the PangoLearn tool
    /// </summary>
    [JsonPropertyName("pangolin_classification")]
    public string? PangolinClassification { get; set; }

    /// <summary>
    /// Assemblies from this location (country or continent)
    /// </summary>
    [JsonPropertyName("geo_location")]
    public string? GeoLocation { get; set; }

    /// <summary>
    /// Assemblies from this state (official two letter code only)
    /// </summary>
    [JsonPropertyName("usa_state")]
    public string? UsaState { get; set; }

    /// <summary>
    /// Only include complete genomes
    /// </summary>
    [JsonPropertyName("complete_only")]
    public bool? CompleteOnly { get; set; }

    /// <summary>
    /// Collection date range start
    /// </summary>
    [JsonPropertyName("collection_date_start")]
    public DateTime? CollectionDateStart { get; set; }

    /// <summary>
    /// Collection date range end
    /// </summary>
    [JsonPropertyName("collection_date_end")]
    public DateTime? CollectionDateEnd { get; set; }

    /// <summary>
    /// Release date range start
    /// </summary>
    [JsonPropertyName("release_date_start")]
    public DateTime? ReleaseDateStart { get; set; }

    /// <summary>
    /// Release date range end
    /// </summary>
    [JsonPropertyName("release_date_end")]
    public DateTime? ReleaseDateEnd { get; set; }
}

/// <summary>
/// Request for virus availability check
/// </summary>
public class VirusAvailabilityRequest
{
    /// <summary>
    /// Virus genome accessions to check
    /// </summary>
    [JsonPropertyName("accessions")]
    public List<string> Accessions { get; set; } = new();
}

/// <summary>
/// Request for SARS-CoV-2 protein datasets
/// </summary>
public class Sars2ProteinDatasetRequest
{
    /// <summary>
    /// SARS-CoV-2 protein names
    /// </summary>
    [JsonPropertyName("proteins")]
    public List<string> Proteins { get; set; } = new();

    /// <summary>
    /// Limit to RefSeq proteins only
    /// </summary>
    [JsonPropertyName("refseq_only")]
    public bool? RefSeqOnly { get; set; }

    /// <summary>
    /// Include annotation types
    /// </summary>
    [JsonPropertyName("include_annotation_type")]
    public List<VirusAnnotationType>? IncludeAnnotationType { get; set; }

    /// <summary>
    /// Output filename
    /// </summary>
    [JsonPropertyName("filename")]
    public string? Filename { get; set; }
}

/// <summary>
/// Request for virus genome table data
/// </summary>
public class VirusGenomeTableRequest
{
    /// <summary>
    /// Taxon for virus genomes
    /// </summary>
    [JsonPropertyName("taxon")]
    public string Taxon { get; set; } = string.Empty;

    /// <summary>
    /// Table format (tsv, csv, jsonl)
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// Limit to RefSeq genomes only
    /// </summary>
    [JsonPropertyName("refseq_only")]
    public bool? RefSeqOnly { get; set; }

    /// <summary>
    /// Only include complete genomes
    /// </summary>
    [JsonPropertyName("complete_only")]
    public bool? CompleteOnly { get; set; }

    /// <summary>
    /// Specify which fields to include in the tabular report
    /// </summary>
    [JsonPropertyName("table_fields")]
    public List<string>? TableFields { get; set; }
}
