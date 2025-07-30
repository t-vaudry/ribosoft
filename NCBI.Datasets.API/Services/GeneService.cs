using Microsoft.Extensions.Logging;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Download;
using NCBI.Datasets.API.Models.Gene;
using NCBI.Datasets.API.Models.Enums;
using System.Text;
using System.Text.Json;

namespace NCBI.Datasets.API.Services;

/// <summary>
/// Service for gene-related operations covering all NCBI Datasets v2 gene endpoints
/// </summary>
public class GeneService
{
    private readonly INCBIDatasetsHttpClient _httpClient;
    private readonly ILogger<GeneService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the GeneService
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls</param>
    /// <param name="logger">Logger instance</param>
    public GeneService(INCBIDatasetsHttpClient httpClient, ILogger<GeneService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };
    }

    #region Gene Reports by ID

    /// <summary>
    /// Get gene reports by GeneID (GET)
    /// </summary>
    /// <param name="geneIds">List of NCBI gene IDs</param>
    /// <param name="returnedContent">Content type to return</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="pageToken">Page token for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gene data report page</returns>
    public async Task<ApiResponse<GeneDataReportPage>> GetGeneReportsByIdAsync(
        IEnumerable<int> geneIds,
        GeneDatasetReportsRequestContentType? returnedContent = null,
        int? pageSize = null,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        if (geneIds == null || !geneIds.Any())
        {
            return ApiResponse<GeneDataReportPage>.Error("Gene IDs cannot be null or empty", 400);
        }

        try
        {
            var geneIdsParam = string.Join(",", geneIds);
            var queryParams = new List<string>();
            
            if (returnedContent.HasValue)
                queryParams.Add($"returned_content={returnedContent.Value}");
            if (pageSize.HasValue)
                queryParams.Add($"page_size={pageSize.Value}");
            if (!string.IsNullOrEmpty(pageToken))
                queryParams.Add($"page_token={Uri.EscapeDataString(pageToken)}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/gene/id/{geneIdsParam}{query}";
            
            _logger.LogDebug("Getting gene reports by ID: {GeneIds}", geneIdsParam);
            
            return await _httpClient.GetAsync<GeneDataReportPage>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene reports by ID");
            return ApiResponse<GeneDataReportPage>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Gene Reports by Accession

    /// <summary>
    /// Get gene reports by accession (GET)
    /// </summary>
    /// <param name="accessions">List of RNA or Protein accessions</param>
    /// <param name="returnedContent">Content type to return</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="pageToken">Page token for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gene data report page</returns>
    public async Task<ApiResponse<GeneDataReportPage>> GetGeneReportsByAccessionAsync(
        IEnumerable<string> accessions,
        GeneDatasetReportsRequestContentType? returnedContent = null,
        int? pageSize = null,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        if (accessions == null || !accessions.Any())
        {
            return ApiResponse<GeneDataReportPage>.Error("Accessions cannot be null or empty", 400);
        }

        try
        {
            var accessionsParam = string.Join(",", accessions);
            var queryParams = new List<string>();
            
            if (returnedContent.HasValue)
                queryParams.Add($"returned_content={returnedContent.Value}");
            if (pageSize.HasValue)
                queryParams.Add($"page_size={pageSize.Value}");
            if (!string.IsNullOrEmpty(pageToken))
                queryParams.Add($"page_token={Uri.EscapeDataString(pageToken)}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/gene/accession/{accessionsParam}{query}";
            
            _logger.LogDebug("Getting gene reports by accession: {Accessions}", accessionsParam);
            
            return await _httpClient.GetAsync<GeneDataReportPage>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene reports by accession");
            return ApiResponse<GeneDataReportPage>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Gene Reports by Symbol and Taxon

    /// <summary>
    /// Get gene reports by symbol and taxon (GET)
    /// </summary>
    /// <param name="symbols">List of gene symbols</param>
    /// <param name="taxon">Taxon for provided gene symbol</param>
    /// <param name="returnedContent">Content type to return</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="pageToken">Page token for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gene data report page</returns>
    public async Task<ApiResponse<GeneDataReportPage>> GetGeneReportsBySymbolAsync(
        IEnumerable<string> symbols,
        string taxon,
        GeneDatasetReportsRequestContentType? returnedContent = null,
        int? pageSize = null,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        if (symbols == null || !symbols.Any())
        {
            return ApiResponse<GeneDataReportPage>.Error("Symbols cannot be null or empty", 400);
        }

        if (string.IsNullOrWhiteSpace(taxon))
        {
            return ApiResponse<GeneDataReportPage>.Error("Taxon cannot be null or empty", 400);
        }

        try
        {
            var symbolsParam = string.Join(",", symbols);
            var queryParams = new List<string>();
            
            if (returnedContent.HasValue)
                queryParams.Add($"returned_content={returnedContent.Value}");
            if (pageSize.HasValue)
                queryParams.Add($"page_size={pageSize.Value}");
            if (!string.IsNullOrEmpty(pageToken))
                queryParams.Add($"page_token={Uri.EscapeDataString(pageToken)}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/gene/symbol/{symbolsParam}/taxon/{Uri.EscapeDataString(taxon)}{query}";
            
            _logger.LogDebug("Getting gene reports by symbol: {Symbols} for taxon: {Taxon}", symbolsParam, taxon);
            
            return await _httpClient.GetAsync<GeneDataReportPage>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene reports by symbol");
            return ApiResponse<GeneDataReportPage>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Gene Reports by Taxon

    /// <summary>
    /// Get gene reports by taxon (GET)
    /// </summary>
    /// <param name="taxon">NCBI Taxonomy ID or name</param>
    /// <param name="returnedContent">Content type to return</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="pageToken">Page token for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gene data report page</returns>
    public async Task<ApiResponse<GeneDataReportPage>> GetGeneReportsByTaxonAsync(
        string taxon,
        GeneDatasetReportsRequestContentType? returnedContent = null,
        int? pageSize = null,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taxon))
        {
            return ApiResponse<GeneDataReportPage>.Error("Taxon cannot be null or empty", 400);
        }

        try
        {
            var queryParams = new List<string>();
            
            if (returnedContent.HasValue)
                queryParams.Add($"returned_content={returnedContent.Value}");
            if (pageSize.HasValue)
                queryParams.Add($"page_size={pageSize.Value}");
            if (!string.IsNullOrEmpty(pageToken))
                queryParams.Add($"page_token={Uri.EscapeDataString(pageToken)}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/gene/taxon/{Uri.EscapeDataString(taxon)}{query}";
            
            _logger.LogDebug("Getting gene reports by taxon: {Taxon}", taxon);
            
            return await _httpClient.GetAsync<GeneDataReportPage>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene reports by taxon");
            return ApiResponse<GeneDataReportPage>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get gene reports by request (POST)
    /// </summary>
    /// <param name="request">Gene dataset reports request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gene data report page</returns>
    public async Task<ApiResponse<GeneDataReportPage>> GetGeneReportsAsync(
        GeneDatasetReportsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<GeneDataReportPage>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/gene";
            _logger.LogDebug("Getting gene reports via POST");
            
            return await _httpClient.PostAsync<GeneDatasetReportsRequest, GeneDataReportPage>(
                endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene reports via POST");
            return ApiResponse<GeneDataReportPage>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Gene Download Operations

    /// <summary>
    /// Download gene dataset by gene IDs (GET)
    /// </summary>
    /// <param name="geneIds">List of NCBI gene IDs</param>
    /// <param name="includeAnnotationType">Annotation types to include</param>
    /// <param name="filename">Output filename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download stream</returns>
    public async Task<ApiResponse<byte[]>> DownloadGeneDatasetByIdAsync(
        IEnumerable<int> geneIds,
        IEnumerable<GeneAnnotationType>? includeAnnotationType = null,
        string? filename = null,
        CancellationToken cancellationToken = default)
    {
        if (geneIds == null || !geneIds.Any())
        {
            return ApiResponse<byte[]>.Error("Gene IDs cannot be null or empty", 400);
        }

        try
        {
            var geneIdsParam = string.Join(",", geneIds);
            var queryParams = new List<string>();
            
            if (includeAnnotationType != null)
            {
                foreach (var annotationType in includeAnnotationType)
                    queryParams.Add($"include_annotation_type={annotationType}");
            }
            if (!string.IsNullOrEmpty(filename))
                queryParams.Add($"filename={Uri.EscapeDataString(filename)}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/gene/id/{geneIdsParam}/download{query}";
            
            _logger.LogDebug("Downloading gene dataset by ID: {GeneIds}", geneIdsParam);
            
            return await _httpClient.DownloadFileAsync(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading gene dataset by ID");
            return ApiResponse<byte[]>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Download gene dataset by request (POST)
    /// </summary>
    /// <param name="request">Gene dataset request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download stream</returns>
    public async Task<ApiResponse<byte[]>> DownloadGeneDatasetAsync(
        GeneDatasetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<byte[]>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/gene/download";
            _logger.LogDebug("Downloading gene dataset via POST");
            
            return await _httpClient.PostDownloadAsync(endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading gene dataset via POST");
            return ApiResponse<byte[]>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Gene Download Summary

    /// <summary>
    /// Get gene download summary by gene IDs (GET)
    /// </summary>
    /// <param name="geneIds">List of NCBI gene IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download summary</returns>
    public async Task<ApiResponse<DownloadSummary>> GetGeneDownloadSummaryByIdAsync(
        IEnumerable<int> geneIds,
        CancellationToken cancellationToken = default)
    {
        if (geneIds == null || !geneIds.Any())
        {
            return ApiResponse<DownloadSummary>.Error("Gene IDs cannot be null or empty", 400);
        }

        try
        {
            var geneIdsParam = string.Join(",", geneIds);
            var endpoint = $"/gene/id/{geneIdsParam}/download_summary";
            
            _logger.LogDebug("Getting gene download summary by ID: {GeneIds}", geneIdsParam);
            
            return await _httpClient.GetAsync<DownloadSummary>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene download summary by ID");
            return ApiResponse<DownloadSummary>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get gene download summary by request (POST)
    /// </summary>
    /// <param name="request">Gene dataset request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download summary</returns>
    public async Task<ApiResponse<DownloadSummary>> GetGeneDownloadSummaryAsync(
        GeneDatasetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<DownloadSummary>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/gene/download_summary";
            _logger.LogDebug("Getting gene download summary via POST");
            
            return await _httpClient.PostAsync<GeneDatasetRequest, DownloadSummary>(
                endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene download summary via POST");
            return ApiResponse<DownloadSummary>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Gene Counts by Taxon

    /// <summary>
    /// Get gene counts by taxon (GET)
    /// </summary>
    /// <param name="taxon">NCBI Taxonomy ID or name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gene counts by taxon</returns>
    public async Task<ApiResponse<GeneCountsByTaxonReply>> GetGeneCountsByTaxonAsync(
        string taxon,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taxon))
        {
            return ApiResponse<GeneCountsByTaxonReply>.Error("Taxon cannot be null or empty", 400);
        }

        try
        {
            var endpoint = $"/gene/taxon/{Uri.EscapeDataString(taxon)}/counts";
            
            _logger.LogDebug("Getting gene counts by taxon: {Taxon}", taxon);
            
            return await _httpClient.GetAsync<GeneCountsByTaxonReply>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene counts by taxon");
            return ApiResponse<GeneCountsByTaxonReply>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get gene counts by taxon (POST)
    /// </summary>
    /// <param name="request">Gene counts by taxon request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gene counts by taxon</returns>
    public async Task<ApiResponse<GeneCountsByTaxonReply>> GetGeneCountsByTaxonAsync(
        GeneCountsByTaxonRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<GeneCountsByTaxonReply>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/gene/taxon/counts";
            _logger.LogDebug("Getting gene counts by taxon via POST");
            
            return await _httpClient.PostAsync<GeneCountsByTaxonRequest, GeneCountsByTaxonReply>(
                endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene counts by taxon via POST");
            return ApiResponse<GeneCountsByTaxonReply>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Gene Orthologs

    /// <summary>
    /// Get gene orthologs by gene ID (GET)
    /// </summary>
    /// <param name="geneId">NCBI gene ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gene orthologs</returns>
    public async Task<ApiResponse<GeneDataReportPage>> GetGeneOrthologsByIdAsync(
        int geneId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"/gene/id/{geneId}/orthologs";
            
            _logger.LogDebug("Getting gene orthologs by ID: {GeneId}", geneId);
            
            return await _httpClient.GetAsync<GeneDataReportPage>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene orthologs by ID");
            return ApiResponse<GeneDataReportPage>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get gene orthologs by request (POST)
    /// </summary>
    /// <param name="request">Ortholog request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gene orthologs</returns>
    public async Task<ApiResponse<GeneDataReportPage>> GetGeneOrthologsAsync(
        OrthologRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<GeneDataReportPage>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/gene/orthologs";
            _logger.LogDebug("Getting gene orthologs via POST");
            
            return await _httpClient.PostAsync<OrthologRequest, GeneDataReportPage>(
                endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene orthologs via POST");
            return ApiResponse<GeneDataReportPage>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Gene Links

    /// <summary>
    /// Get gene links by gene IDs (GET)
    /// </summary>
    /// <param name="geneIds">List of NCBI gene IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gene links</returns>
    public async Task<ApiResponse<GeneLinksReply>> GetGeneLinksByIdAsync(
        IEnumerable<int> geneIds,
        CancellationToken cancellationToken = default)
    {
        if (geneIds == null || !geneIds.Any())
        {
            return ApiResponse<GeneLinksReply>.Error("Gene IDs cannot be null or empty", 400);
        }

        try
        {
            var geneIdsParam = string.Join(",", geneIds);
            var endpoint = $"/gene/id/{geneIdsParam}/links";
            
            _logger.LogDebug("Getting gene links by ID: {GeneIds}", geneIdsParam);
            
            return await _httpClient.GetAsync<GeneLinksReply>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene links by ID");
            return ApiResponse<GeneLinksReply>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get gene links by request (POST)
    /// </summary>
    /// <param name="request">Gene links request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gene links</returns>
    public async Task<ApiResponse<GeneLinksReply>> GetGeneLinksAsync(
        GeneLinksRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<GeneLinksReply>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/gene/links";
            _logger.LogDebug("Getting gene links via POST");
            
            return await _httpClient.PostAsync<GeneLinksRequest, GeneLinksReply>(
                endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene links via POST");
            return ApiResponse<GeneLinksReply>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Gene Chromosome Summary

    /// <summary>
    /// Get gene chromosome summary (GET)
    /// </summary>
    /// <param name="taxon">NCBI Taxonomy ID or name</param>
    /// <param name="annotationName">Annotation name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gene chromosome summary</returns>
    public async Task<ApiResponse<GeneChromosomeSummaryReply>> GetGeneChromosomeSummaryAsync(
        string taxon,
        string annotationName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taxon))
        {
            return ApiResponse<GeneChromosomeSummaryReply>.Error("Taxon cannot be null or empty", 400);
        }

        if (string.IsNullOrWhiteSpace(annotationName))
        {
            return ApiResponse<GeneChromosomeSummaryReply>.Error("Annotation name cannot be null or empty", 400);
        }

        try
        {
            var endpoint = $"/gene/taxon/{Uri.EscapeDataString(taxon)}/annotation/{Uri.EscapeDataString(annotationName)}/chromosome_summary";
            
            _logger.LogDebug("Getting gene chromosome summary for taxon: {Taxon}, annotation: {AnnotationName}", taxon, annotationName);
            
            return await _httpClient.GetAsync<GeneChromosomeSummaryReply>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gene chromosome summary");
            return ApiResponse<GeneChromosomeSummaryReply>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion
}
