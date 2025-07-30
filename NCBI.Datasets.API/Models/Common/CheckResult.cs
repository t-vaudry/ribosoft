using System.Text.Json.Serialization;

namespace NCBI.Datasets.API.Models.Common;

/// <summary>
/// Result of a dataset check operation
/// </summary>
public class CheckResult
{
    /// <summary>
    /// Whether the dataset is valid
    /// </summary>
    [JsonPropertyName("is_valid")]
    public bool IsValid { get; set; }

    /// <summary>
    /// Optional message providing details about the check result
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// List of validation errors if any
    /// </summary>
    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }

    /// <summary>
    /// List of validation warnings if any
    /// </summary>
    [JsonPropertyName("warnings")]
    public List<string>? Warnings { get; set; }
}
