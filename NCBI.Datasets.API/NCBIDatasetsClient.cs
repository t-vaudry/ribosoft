using Microsoft.Extensions.Logging;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Download;
using NCBI.Datasets.API.Models.Enums;
using NCBI.Datasets.API.Models.Genome;
using NCBI.Datasets.API.Services;

namespace NCBI.Datasets.API;

/// <summary>
/// Main client for NCBI Datasets API
/// </summary>
public class NCBIDatasetsClient : INCBIDatasetsClient
{
    private readonly GenomeService _genomeService;
    private readonly NCBIDatasetsHttpClient _httpClient;
    private readonly ILogger<NCBIDatasetsClient> _logger;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the NCBIDatasetsClient
    /// </summary>
    /// <param name="genomeService">Genome service</param>
    /// <param name="httpClient">HTTP client</param>
    /// <param name="logger">Logger instance</param>
    public NCBIDatasetsClient(
        GenomeService genomeService,
        NCBIDatasetsHttpClient httpClient,
        ILogger<NCBIDatasetsClient> logger)
    {
        _genomeService = genomeService ?? throw new ArgumentNullException(nameof(genomeService));
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
}
