using Microsoft.Extensions.Logging;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Download;
using NCBI.Datasets.API.Models.Genome;
using NCBI.Datasets.API.Models.Enums;
using System.Text;
using System.Text.Json;

namespace NCBI.Datasets.API.Services;

/// <summary>
/// Service for genome-related operations covering all NCBI Datasets v2 genome endpoints
/// </summary>
public class GenomeService
{
    private readonly INCBIDatasetsHttpClient _httpClient;
    private readonly ILogger<GenomeService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the GenomeService
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls</param>
    /// <param name="logger">Logger instance</param>
    public GenomeService(INCBIDatasetsHttpClient httpClient, ILogger<GenomeService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };
    }

    #region Assembly Dataset Operations

    /// <summary>
    /// Check assembly dataset availability by accessions (GET)
    /// </summary>
    /// <param name="accessions">List of assembly accessions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assembly dataset availability</returns>
    public async Task<ApiResponse<AssemblyDatasetAvailability>> CheckAssemblyDatasetAvailabilityAsync(
        IEnumerable<string> accessions,
        CancellationToken cancellationToken = default)
    {
        if (accessions == null || !accessions.Any())
        {
            return ApiResponse<AssemblyDatasetAvailability>.Error("Accessions cannot be null or empty", 400);
        }

        try
        {
            var accessionsParam = string.Join(",", accessions);
            var endpoint = $"/genome/dataset/{accessionsParam}";
            _logger.LogDebug("Checking assembly dataset availability for accessions: {Accessions}", accessionsParam);
            
            return await _httpClient.GetAsync<AssemblyDatasetAvailability>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking assembly dataset availability");
            return ApiResponse<AssemblyDatasetAvailability>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Check assembly dataset availability by request (POST)
    /// </summary>
    /// <param name="request">Assembly dataset request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assembly dataset availability</returns>
    public async Task<ApiResponse<AssemblyDatasetAvailability>> CheckAssemblyDatasetAvailabilityAsync(
        AssemblyDatasetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<AssemblyDatasetAvailability>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/genome/dataset";
            _logger.LogDebug("Checking assembly dataset availability via POST");
            
            return await _httpClient.PostAsync<AssemblyDatasetRequest, AssemblyDatasetAvailability>(
                endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking assembly dataset availability via POST");
            return ApiResponse<AssemblyDatasetAvailability>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Assembly Dataset Reports

    /// <summary>
    /// Get assembly dataset reports by accessions (GET)
    /// </summary>
    /// <param name="accessions">List of assembly accessions</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="pageToken">Page token for pagination</param>
    /// <param name="returnedContent">Content type to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assembly dataset report</returns>
    public async Task<ApiResponse<AssemblyDatasetReport>> GetAssemblyDatasetReportsAsync(
        IEnumerable<string> accessions,
        int? pageSize = null,
        string? pageToken = null,
        AssemblyDatasetReportsRequestContentType? returnedContent = null,
        CancellationToken cancellationToken = default)
    {
        if (accessions == null || !accessions.Any())
        {
            return ApiResponse<AssemblyDatasetReport>.Error("Accessions cannot be null or empty", 400);
        }

        try
        {
            var accessionsParam = string.Join(",", accessions);
            var queryParams = new List<string>();
            
            if (pageSize.HasValue)
                queryParams.Add($"page_size={pageSize.Value}");
            if (!string.IsNullOrEmpty(pageToken))
                queryParams.Add($"page_token={Uri.EscapeDataString(pageToken)}");
            if (returnedContent.HasValue)
                queryParams.Add($"returned_content={returnedContent.Value}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/genome/dataset_report/{accessionsParam}{query}";
            
            _logger.LogDebug("Getting assembly dataset reports for accessions: {Accessions}", accessionsParam);
            
            return await _httpClient.GetAsync<AssemblyDatasetReport>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assembly dataset reports");
            return ApiResponse<AssemblyDatasetReport>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get assembly dataset reports by request (POST)
    /// </summary>
    /// <param name="request">Assembly dataset request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assembly dataset report</returns>
    public async Task<ApiResponse<AssemblyDatasetReport>> GetAssemblyDatasetReportsAsync(
        AssemblyDatasetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<AssemblyDatasetReport>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/genome/dataset_report";
            _logger.LogDebug("Getting assembly dataset reports via POST");
            
            return await _httpClient.PostAsync<AssemblyDatasetRequest, AssemblyDatasetReport>(
                endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assembly dataset reports via POST");
            return ApiResponse<AssemblyDatasetReport>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Download Operations

    /// <summary>
    /// Download genome dataset by accessions (GET)
    /// </summary>
    /// <param name="accessions">List of assembly accessions</param>
    /// <param name="includeAnnotationType">Annotation types to include</param>
    /// <param name="hydrated">Hydration level</param>
    /// <param name="filename">Output filename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download stream</returns>
    public async Task<ApiResponse<byte[]>> DownloadGenomeDatasetAsync(
        IEnumerable<string> accessions,
        IEnumerable<AnnotationTypes>? includeAnnotationType = null,
        HydrationLevel? hydrated = null,
        string? filename = null,
        CancellationToken cancellationToken = default)
    {
        if (accessions == null || !accessions.Any())
        {
            return ApiResponse<byte[]>.Error("Accessions cannot be null or empty", 400);
        }

        try
        {
            var accessionsParam = string.Join(",", accessions);
            var queryParams = new List<string>();
            
            if (includeAnnotationType != null)
            {
                foreach (var annotationType in includeAnnotationType)
                    queryParams.Add($"include_annotation_type={annotationType}");
            }
            if (hydrated.HasValue)
                queryParams.Add($"hydrated={hydrated.Value}");
            if (!string.IsNullOrEmpty(filename))
                queryParams.Add($"filename={Uri.EscapeDataString(filename)}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/genome/accession/{accessionsParam}/download{query}";
            
            _logger.LogDebug("Downloading genome dataset for accessions: {Accessions}", accessionsParam);
            
            return await _httpClient.DownloadFileAsync(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading genome dataset");
            return ApiResponse<byte[]>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Download genome dataset by request (POST)
    /// </summary>
    /// <param name="request">Genome download request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download stream</returns>
    public async Task<ApiResponse<byte[]>> DownloadGenomeDatasetAsync(
        GenomeDownloadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<byte[]>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/genome/accession/download";
            _logger.LogDebug("Downloading genome dataset via POST");
            
            return await _httpClient.PostDownloadAsync(endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading genome dataset via POST");
            return ApiResponse<byte[]>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Summary Operations

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

    #endregion

    #region Assembly Links

    /// <summary>
    /// Get assembly links by accessions (GET)
    /// </summary>
    /// <param name="accessions">List of assembly accessions</param>
    /// <param name="linkType">Type of links to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assembly links</returns>
    public async Task<ApiResponse<AssemblyLinksReply>> GetAssemblyLinksAsync(
        IEnumerable<string> accessions,
        AssemblyLinkType? linkType = null,
        CancellationToken cancellationToken = default)
    {
        if (accessions == null || !accessions.Any())
        {
            return ApiResponse<AssemblyLinksReply>.Error("Accessions cannot be null or empty", 400);
        }

        try
        {
            var accessionsParam = string.Join(",", accessions);
            var query = linkType.HasValue ? $"?link_type={linkType.Value}" : "";
            var endpoint = $"/genome/accession/{accessionsParam}/links{query}";
            
            _logger.LogDebug("Getting assembly links for accessions: {Accessions}", accessionsParam);
            
            return await _httpClient.GetAsync<AssemblyLinksReply>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assembly links");
            return ApiResponse<AssemblyLinksReply>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Sequence Operations

    /// <summary>
    /// Get sequence assemblies by sequence accessions (GET)
    /// </summary>
    /// <param name="sequenceAccessions">List of sequence accessions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sequence assemblies</returns>
    public async Task<ApiResponse<SequenceAssembliesReply>> GetSequenceAssembliesAsync(
        IEnumerable<string> sequenceAccessions,
        CancellationToken cancellationToken = default)
    {
        if (sequenceAccessions == null || !sequenceAccessions.Any())
        {
            return ApiResponse<SequenceAssembliesReply>.Error("Sequence accessions cannot be null or empty", 400);
        }

        try
        {
            var accessionsParam = string.Join(",", sequenceAccessions);
            var endpoint = $"/genome/sequence/{accessionsParam}/assemblies";
            
            _logger.LogDebug("Getting sequence assemblies for accessions: {Accessions}", accessionsParam);
            
            return await _httpClient.GetAsync<SequenceAssembliesReply>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sequence assemblies");
            return ApiResponse<SequenceAssembliesReply>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get sequence reports by sequence accessions (GET)
    /// </summary>
    /// <param name="sequenceAccessions">List of sequence accessions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sequence reports</returns>
    public async Task<ApiResponse<SequenceReportsReply>> GetSequenceReportsAsync(
        IEnumerable<string> sequenceAccessions,
        CancellationToken cancellationToken = default)
    {
        if (sequenceAccessions == null || !sequenceAccessions.Any())
        {
            return ApiResponse<SequenceReportsReply>.Error("Sequence accessions cannot be null or empty", 400);
        }

        try
        {
            var accessionsParam = string.Join(",", sequenceAccessions);
            var endpoint = $"/genome/sequence/{accessionsParam}/reports";
            
            _logger.LogDebug("Getting sequence reports for accessions: {Accessions}", accessionsParam);
            
            return await _httpClient.GetAsync<SequenceReportsReply>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sequence reports");
            return ApiResponse<SequenceReportsReply>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region CheckM Operations

    /// <summary>
    /// Get CheckM histogram by species taxon (GET)
    /// </summary>
    /// <param name="speciesTaxon">Species taxon identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CheckM histogram</returns>
    public async Task<ApiResponse<AssemblyCheckMHistogramReply>> GetCheckMHistogramAsync(
        string speciesTaxon,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(speciesTaxon))
        {
            return ApiResponse<AssemblyCheckMHistogramReply>.Error("Species taxon cannot be null or empty", 400);
        }

        try
        {
            var endpoint = $"/genome/taxon/{Uri.EscapeDataString(speciesTaxon)}/checkm_histogram";
            
            _logger.LogDebug("Getting CheckM histogram for species taxon: {SpeciesTaxon}", speciesTaxon);
            
            return await _httpClient.GetAsync<AssemblyCheckMHistogramReply>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CheckM histogram for species taxon: {SpeciesTaxon}", speciesTaxon);
            return ApiResponse<AssemblyCheckMHistogramReply>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Check Operations

    /// <summary>
    /// Check genome dataset by accessions (GET)
    /// </summary>
    /// <param name="accessions">List of assembly accessions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Check result</returns>
    public async Task<ApiResponse<CheckResult>> CheckGenomeDatasetAsync(
        IEnumerable<string> accessions,
        CancellationToken cancellationToken = default)
    {
        if (accessions == null || !accessions.Any())
        {
            return ApiResponse<CheckResult>.Error("Accessions cannot be null or empty", 400);
        }

        try
        {
            var accessionsParam = string.Join(",", accessions);
            var endpoint = $"/genome/accession/{accessionsParam}/check";
            
            _logger.LogDebug("Checking genome dataset for accessions: {Accessions}", accessionsParam);
            
            return await _httpClient.GetAsync<CheckResult>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking genome dataset");
            return ApiResponse<CheckResult>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Check genome dataset by request (POST)
    /// </summary>
    /// <param name="request">Genome check request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Check result</returns>
    public async Task<ApiResponse<CheckResult>> CheckGenomeDatasetAsync(
        GenomeCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<CheckResult>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/genome/accession/check";
            _logger.LogDebug("Checking genome dataset via POST");
            
            return await _httpClient.PostAsync<GenomeCheckRequest, CheckResult>(
                endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking genome dataset via POST");
            return ApiResponse<CheckResult>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Legacy Download Operations

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

    #endregion

    #region Private Helper Methods

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

    #endregion
}
