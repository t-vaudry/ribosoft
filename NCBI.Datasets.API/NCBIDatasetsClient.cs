using Microsoft.Extensions.Logging;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Download;
using NCBI.Datasets.API.Models.Enums;
using NCBI.Datasets.API.Models.Genome;
using NCBI.Datasets.API.Models.Responses;
using NCBI.Datasets.API.Services;

namespace NCBI.Datasets.API;

/// <summary>
/// Main client for NCBI Datasets API
/// </summary>
public class NCBIDatasetsClient : INCBIDatasetsClient
{
    private readonly GenomeService _genomeService;
    private readonly TaxonomyService _taxonomyService;
    private readonly NCBIDatasetsHttpClient _httpClient;
    private readonly ILogger<NCBIDatasetsClient> _logger;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the NCBIDatasetsClient
    /// </summary>
    /// <param name="genomeService">Genome service</param>
    /// <param name="taxonomyService">Taxonomy service</param>
    /// <param name="httpClient">HTTP client</param>
    /// <param name="logger">Logger instance</param>
    public NCBIDatasetsClient(
        GenomeService genomeService,
        TaxonomyService taxonomyService,
        NCBIDatasetsHttpClient httpClient,
        ILogger<NCBIDatasetsClient> logger)
    {
        _genomeService = genomeService ?? throw new ArgumentNullException(nameof(genomeService));
        _taxonomyService = taxonomyService ?? throw new ArgumentNullException(nameof(taxonomyService));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ApiResponse<DownloadSummary>> GetGenomeDownloadSummaryAsync(
        GenomeDownloadSummaryRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<DownloadSummary>.Error("Request cannot be null", 400);
        }

        _logger.LogDebug("Getting genome download summary for {Count} accessions", request.Accessions?.Count ?? 0);
        return await _genomeService.GetDownloadSummaryAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<DownloadSummary>> GetGenomeDownloadSummaryByPostAsync(
        GenomeDownloadSummaryRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<DownloadSummary>.Error("Request cannot be null", 400);
        }

        _logger.LogDebug("Getting genome download summary via POST for {Count} accessions", request.Accessions?.Count ?? 0);
        return await _genomeService.GetDownloadSummaryByPostAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<byte[]>> DownloadGenomeDatasetAsync(
        string downloadUrl, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Downloading genome dataset from URL");
        return await _genomeService.DownloadDatasetAsync(downloadUrl, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<DownloadSummary>> GetGenomeDownloadSummaryAsync(
        string accession,
        IEnumerable<string>? chromosomes = null,
        IEnumerable<AnnotationForAssemblyType>? annotationTypes = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accession))
        {
            return ApiResponse<DownloadSummary>.Error("Accession cannot be null or empty", 400);
        }

        var request = new GenomeDownloadSummaryRequest
        {
            Accessions = new List<string> { accession },
            Chromosomes = chromosomes?.ToList(),
            IncludeAnnotationTypes = annotationTypes?.ToList()
        };

        return await GetGenomeDownloadSummaryAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<DownloadSummary>> GetGenomeDownloadSummaryAsync(
        IEnumerable<string> accessions,
        IEnumerable<string>? chromosomes = null,
        IEnumerable<AnnotationForAssemblyType>? annotationTypes = null,
        CancellationToken cancellationToken = default)
    {
        if (accessions == null)
        {
            return ApiResponse<DownloadSummary>.Error("Accessions cannot be null", 400);
        }

        var accessionList = accessions.ToList();
        if (accessionList.Count == 0)
        {
            return ApiResponse<DownloadSummary>.Error("At least one accession is required", 400);
        }

        var request = new GenomeDownloadSummaryRequest
        {
            Accessions = accessionList,
            Chromosomes = chromosomes?.ToList(),
            IncludeAnnotationTypes = annotationTypes?.ToList()
        };

        return await GetGenomeDownloadSummaryAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<TaxonomySuggestionResponse>> GetTaxonomySuggestionsAsync(
        string taxonQuery,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taxonQuery))
        {
            return ApiResponse<TaxonomySuggestionResponse>.Error("Taxon query cannot be null or empty", 400);
        }

        _logger.LogDebug("Getting taxonomy suggestions for query: {Query}", SanitizeForLogging(taxonQuery));
        return await _taxonomyService.GetTaxonomySuggestionsByQueryAsync(taxonQuery, limit);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<AssemblyDatasetReport>> GetAssemblyDatasetReportsByTaxonAsync(
        IEnumerable<int> taxons,
        int? pageSize = null,
        Models.Enums.AssemblyDatasetReportsRequestContentType? returnedContent = null,
        CancellationToken cancellationToken = default)
    {
        if (taxons == null)
        {
            return ApiResponse<AssemblyDatasetReport>.Error("Taxons cannot be null", 400);
        }

        var taxonList = taxons.ToList();
        if (taxonList.Count == 0)
        {
            return ApiResponse<AssemblyDatasetReport>.Error("At least one taxon is required", 400);
        }

        _logger.LogDebug("Getting assembly dataset reports for {Count} taxons with content type: {ContentType}", 
            taxonList.Count, returnedContent?.ToString() ?? "default");
        return await _genomeService.GetAssemblyDatasetReportsByTaxonAsync(taxonList, pageSize, null, returnedContent, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<AssemblyDatasetReport>> GetAssemblyDatasetReportsAsync(
        IEnumerable<string> accessions,
        int? pageSize = null,
        Models.Enums.AssemblyDatasetReportsRequestContentType? returnedContent = null,
        CancellationToken cancellationToken = default)
    {
        if (accessions == null)
        {
            return ApiResponse<AssemblyDatasetReport>.Error("Accessions cannot be null", 400);
        }

        var accessionList = accessions.ToList();
        if (accessionList.Count == 0)
        {
            return ApiResponse<AssemblyDatasetReport>.Error("At least one accession is required", 400);
        }

        _logger.LogDebug("Getting assembly dataset reports for {Count} accessions with content type: {ContentType}", 
            accessionList.Count, returnedContent?.ToString() ?? "default");
        return await _genomeService.GetAssemblyDatasetReportsAsync(accessionList, pageSize, null, returnedContent, cancellationToken);
    }

    /// <summary>
    /// Disposes the client
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose method
    /// </summary>
    /// <param name="disposing">Whether disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }

    /*! \fn SanitizeForLogging
     * \brief Sanitize user input for safe logging to prevent log injection attacks
     * \param input User-provided input that may contain malicious content
     * \return Sanitized string safe for logging
     */
    private static string? SanitizeForLogging(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        // Remove or replace characters that could be used for log injection
        return input
            .Replace('\r', ' ')  // Remove carriage returns
            .Replace('\n', ' ')  // Remove line feeds
            .Replace('\t', ' ')  // Replace tabs with spaces
            .Trim();             // Remove leading/trailing whitespace
    }
}
