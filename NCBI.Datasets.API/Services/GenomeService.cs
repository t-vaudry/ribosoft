using Microsoft.Extensions.Logging;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Download;
using NCBI.Datasets.API.Models.Genome;
using System.Text;

namespace NCBI.Datasets.API.Services;

/// <summary>
/// Service for genome-related operations
/// </summary>
public class GenomeService
{
    private readonly NCBIDatasetsHttpClient _httpClient;
    private readonly ILogger<GenomeService> _logger;

    /// <summary>
    /// Initializes a new instance of the GenomeService
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls</param>
    /// <param name="logger">Logger instance</param>
    public GenomeService(NCBIDatasetsHttpClient httpClient, ILogger<GenomeService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets download summary for genome assemblies by accession
    /// </summary>
    /// <param name="request">Download summary request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download summary response</returns>
    public async Task<ApiResponse<DownloadSummary>> GetDownloadSummaryAsync(
        GenomeDownloadSummaryRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<DownloadSummary>.Error("Request cannot be null", 400);
        }

        if (!request.IsValid())
        {
            return ApiResponse<DownloadSummary>.Error("Request is invalid: accessions are required", 400);
        }

        try
        {
            // For small requests (single accession), use GET
            if (request.Accessions.Count == 1)
            {
                var endpoint = BuildGetEndpoint(request);
                _logger.LogDebug("Getting download summary for single accession: {Accession}", request.Accessions[0]);
                return await _httpClient.GetAsync<DownloadSummary>(endpoint, cancellationToken);
            }
            else
            {
                // For multiple accessions, use POST
                _logger.LogDebug("Getting download summary for {Count} accessions", request.Accessions.Count);
                return await GetDownloadSummaryByPostAsync(request, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting download summary for accessions: {Accessions}", 
                string.Join(", ", request.Accessions));
            return ApiResponse<DownloadSummary>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets download summary using POST method (for larger requests)
    /// </summary>
    /// <param name="request">Download summary request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download summary response</returns>
    public async Task<ApiResponse<DownloadSummary>> GetDownloadSummaryByPostAsync(
        GenomeDownloadSummaryRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<DownloadSummary>.Error("Request cannot be null", 400);
        }

        if (!request.IsValid())
        {
            return ApiResponse<DownloadSummary>.Error("Request is invalid: accessions are required", 400);
        }

        try
        {
            var postRequest = new GenomeDownloadSummaryPostRequest
            {
                Accessions = request.Accessions,
                Chromosomes = request.Chromosomes,
                IncludeAnnotationTypes = request.IncludeAnnotationTypes
            };

            const string endpoint = "/genome/download_summary";
            _logger.LogDebug("Getting download summary via POST for {Count} accessions", request.Accessions.Count);
            
            return await _httpClient.PostAsync<GenomeDownloadSummaryPostRequest, DownloadSummary>(
                endpoint, postRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting download summary via POST for accessions: {Accessions}", 
                string.Join(", ", request.Accessions));
            return ApiResponse<DownloadSummary>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Downloads genome dataset
    /// </summary>
    /// <param name="downloadUrl">Download URL from summary</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Downloaded file content</returns>
    public async Task<ApiResponse<byte[]>> DownloadDatasetAsync(
        string downloadUrl, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(downloadUrl))
        {
            return ApiResponse<byte[]>.Error("Download URL cannot be null or empty", 400);
        }

        try
        {
            _logger.LogDebug("Downloading genome dataset from: {Url}", downloadUrl);
            return await _httpClient.DownloadFileAsync(downloadUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading dataset from URL: {Url}", downloadUrl);
            return ApiResponse<byte[]>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Builds the GET endpoint URL with query parameters
    /// </summary>
    /// <param name="request">Download summary request</param>
    /// <returns>Endpoint URL</returns>
    private string BuildGetEndpoint(GenomeDownloadSummaryRequest request)
    {
        var accession = request.Accessions[0];
        var endpoint = new StringBuilder($"/genome/accession/{accession}/download_summary");
        
        var queryParams = new List<string>();

        // Add chromosomes if specified
        if (request.Chromosomes?.Count > 0)
        {
            foreach (var chromosome in request.Chromosomes)
            {
                queryParams.Add($"chromosomes={Uri.EscapeDataString(chromosome)}");
            }
        }

        // Add annotation types if specified
        if (request.IncludeAnnotationTypes?.Count > 0)
        {
            foreach (var annotationType in request.IncludeAnnotationTypes)
            {
                queryParams.Add($"include_annotation_type={annotationType}");
            }
        }

        // Append query parameters
        if (queryParams.Count > 0)
        {
            endpoint.Append("?");
            endpoint.Append(string.Join("&", queryParams));
        }

        return endpoint.ToString();
    }
}
