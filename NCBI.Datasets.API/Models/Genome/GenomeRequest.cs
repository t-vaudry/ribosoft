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
