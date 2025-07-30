using NCBI.Datasets.API.Models.Enums;
using System.Text.Json.Serialization;

namespace NCBI.Datasets.API.Models.Genome;

/// <summary>
/// Request for genome download summary by accession
/// </summary>
public class GenomeDownloadSummaryRequest
{
    /// <summary>
    /// NCBI genome assembly accessions
    /// </summary>
    public List<string> Accessions { get; set; } = new();

    /// <summary>
    /// Specific chromosomes to include (optional)
    /// </summary>
    public List<string>? Chromosomes { get; set; }

    /// <summary>
    /// Additional annotation types to include (optional)
    /// </summary>
    public List<AnnotationForAssemblyType>? IncludeAnnotationTypes { get; set; }

    /// <summary>
    /// Validates the request
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return Accessions.Count > 0 && Accessions.All(a => !string.IsNullOrWhiteSpace(a));
    }
}

/// <summary>
/// Request for genome download summary by POST (for larger requests)
/// </summary>
public class GenomeDownloadSummaryPostRequest
{
    /// <summary>
    /// NCBI genome assembly accessions
    /// </summary>
    [JsonPropertyName("accessions")]
    public List<string> Accessions { get; set; } = new();

    /// <summary>
    /// Specific chromosomes to include (optional)
    /// </summary>
    [JsonPropertyName("chromosomes")]
    public List<string>? Chromosomes { get; set; }

    /// <summary>
    /// Additional annotation types to include (optional)
    /// </summary>
    [JsonPropertyName("include_annotation_type")]
    public List<AnnotationForAssemblyType>? IncludeAnnotationTypes { get; set; }

    /// <summary>
    /// Validates the request
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return Accessions.Count > 0 && Accessions.All(a => !string.IsNullOrWhiteSpace(a));
    }
}

/// <summary>
/// Full assembly dataset request for downloads and reports
/// </summary>
public class AssemblyDatasetRequest
{
    /// <summary>
    /// NCBI genome assembly accessions
    /// </summary>
    [JsonPropertyName("accessions")]
    public List<string> Accessions { get; set; } = new();

    /// <summary>
    /// Specific chromosomes to include (optional)
    /// </summary>
    [JsonPropertyName("chromosomes")]
    public List<string>? Chromosomes { get; set; }

    /// <summary>
    /// Additional annotation types to include (optional)
    /// </summary>
    [JsonPropertyName("include_annotation_type")]
    public List<AnnotationForAssemblyType>? IncludeAnnotationTypes { get; set; }

    /// <summary>
    /// Dataset resolution type
    /// </summary>
    [JsonPropertyName("hydrated")]
    public AssemblyDatasetRequestResolution? Hydrated { get; set; }

    /// <summary>
    /// Include TSV representation of the data report
    /// </summary>
    [JsonPropertyName("include_tsv")]
    public bool? IncludeTsv { get; set; }

    /// <summary>
    /// Validates the request
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return Accessions.Count > 0 && Accessions.All(a => !string.IsNullOrWhiteSpace(a));
    }
}

/// <summary>
/// Request for genome dataset reports by various identifiers
/// </summary>
public class GenomeDatasetReportRequest
{
    /// <summary>
    /// Assembly accessions
    /// </summary>
    public List<string>? Accessions { get; set; }

    /// <summary>
    /// Taxon IDs
    /// </summary>
    public List<string>? Taxons { get; set; }

    /// <summary>
    /// BioProject accessions
    /// </summary>
    public List<string>? Bioprojects { get; set; }

    /// <summary>
    /// BioSample IDs
    /// </summary>
    public List<string>? BiosampleIds { get; set; }

    /// <summary>
    /// WGS accessions
    /// </summary>
    public List<string>? WgsAccessions { get; set; }

    /// <summary>
    /// Assembly names
    /// </summary>
    public List<string>? AssemblyNames { get; set; }

    /// <summary>
    /// Page size for pagination
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Page token for pagination
    /// </summary>
    public string? PageToken { get; set; }

    /// <summary>
    /// Content type for the report
    /// </summary>
    public AssemblyDatasetReportsRequestContentType? ContentType { get; set; }

    /// <summary>
    /// Include tabular header
    /// </summary>
    public bool? IncludeTabularHeader { get; set; }

    /// <summary>
    /// Table format template
    /// </summary>
    public string? TableFormat { get; set; }
}

/// <summary>
/// Assembly accessions wrapper
/// </summary>
public class AssemblyAccessions
{
    /// <summary>
    /// List of assembly accessions
    /// </summary>
    [JsonPropertyName("accessions")]
    public List<string> Accessions { get; set; } = new();
}

/// <summary>
/// Request for genome download with filename
/// </summary>
public class GenomeDownloadRequest : AssemblyDatasetRequest
{
    /// <summary>
    /// Output filename
    /// </summary>
    public string? Filename { get; set; }
}

/// <summary>
/// Request for checking genome availability
/// </summary>
public class GenomeCheckRequest
{
    /// <summary>
    /// Assembly accessions to check
    /// </summary>
    [JsonPropertyName("accessions")]
    public List<string> Accessions { get; set; } = new();

    /// <summary>
    /// Validates the request
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return Accessions.Count > 0 && Accessions.All(a => !string.IsNullOrWhiteSpace(a));
    }
}
