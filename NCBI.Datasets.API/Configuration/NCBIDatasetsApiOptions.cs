namespace NCBI.Datasets.API.Configuration;

/// <summary>
/// Configuration options for the NCBI Datasets API client
/// </summary>
public class NCBIDatasetsApiOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "NCBIDatasetsApi";

    /// <summary>
    /// Base URL for the NCBI Datasets API
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.ncbi.nlm.nih.gov/datasets/v2";

    /// <summary>
    /// API key for authentication
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// HTTP request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of retry attempts for failed requests
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts in seconds
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 1;

    /// <summary>
    /// Validates the configuration options
    /// </summary>
    /// <returns>True if configuration is valid, false otherwise</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(BaseUrl) &&
               !string.IsNullOrWhiteSpace(ApiKey) &&
               TimeoutSeconds > 0 &&
               MaxRetryAttempts >= 0 &&
               RetryDelaySeconds >= 0;
    }
}
