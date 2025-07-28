using Microsoft.Extensions.Logging;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Download;
using NCBI.Datasets.API.Models.Virus;
using NCBI.Datasets.API.Models.Enums;
using System.Text;
using System.Text.Json;

namespace NCBI.Datasets.API.Services;

/// <summary>
/// Service for virus-related operations covering all NCBI Datasets v2 virus endpoints
/// </summary>
public class VirusService
{
    private readonly INCBIDatasetsHttpClient _httpClient;
    private readonly ILogger<VirusService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the VirusService
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls</param>
    /// <param name="logger">Logger instance</param>
    public VirusService(INCBIDatasetsHttpClient httpClient, ILogger<VirusService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };
    }

    #region Virus Genome Summary

    /// <summary>
    /// Get summary data for virus genomes by taxon (GET)
    /// </summary>
    /// <param name="taxon">NCBI Taxonomy ID or name</param>
    /// <param name="refSeqOnly">Limit to RefSeq genomes only</param>
    /// <param name="completeOnly">Only include complete genomes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download summary</returns>
    public async Task<ApiResponse<DownloadSummary>> GetVirusGenomeSummaryByTaxonAsync(
        string taxon,
        bool? refSeqOnly = null,
        bool? completeOnly = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taxon))
        {
            return ApiResponse<DownloadSummary>.Error("Taxon cannot be null or empty", 400);
        }

        try
        {
            var queryParams = new List<string>();
            
            if (refSeqOnly.HasValue)
                queryParams.Add($"refseq_only={refSeqOnly.Value.ToString().ToLower()}");
            if (completeOnly.HasValue)
                queryParams.Add($"complete_only={completeOnly.Value.ToString().ToLower()}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/virus/taxon/{Uri.EscapeDataString(taxon)}/genome{query}";
            
            _logger.LogDebug("Getting virus genome summary by taxon: {Taxon}", taxon);
            
            return await _httpClient.GetAsync<DownloadSummary>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting virus genome summary by taxon");
            return ApiResponse<DownloadSummary>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get summary data for virus genomes by request (POST)
    /// </summary>
    /// <param name="request">Virus dataset request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download summary</returns>
    public async Task<ApiResponse<DownloadSummary>> GetVirusGenomeSummaryAsync(
        VirusDatasetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<DownloadSummary>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/virus/genome";
            _logger.LogDebug("Getting virus genome summary via POST");
            
            return await _httpClient.PostAsync<VirusDatasetRequest, DownloadSummary>(
                endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting virus genome summary via POST");
            return ApiResponse<DownloadSummary>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region SARS-CoV-2 Protein Operations

    /// <summary>
    /// Get summary of SARS-CoV-2 protein datasets by protein name (GET)
    /// </summary>
    /// <param name="proteins">List of protein names</param>
    /// <param name="refSeqOnly">Limit to RefSeq proteins only</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SARS-CoV-2 protein dataset summary</returns>
    public async Task<ApiResponse<Sars2ProteinDatasetSummary>> GetSars2ProteinSummaryAsync(
        IEnumerable<string> proteins,
        bool? refSeqOnly = null,
        CancellationToken cancellationToken = default)
    {
        if (proteins == null || !proteins.Any())
        {
            return ApiResponse<Sars2ProteinDatasetSummary>.Error("Proteins cannot be null or empty", 400);
        }

        try
        {
            var proteinsParam = string.Join(",", proteins);
            var queryParams = new List<string>();
            
            if (refSeqOnly.HasValue)
                queryParams.Add($"refseq_only={refSeqOnly.Value.ToString().ToLower()}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/virus/taxon/sars2/protein/{proteinsParam}{query}";
            
            _logger.LogDebug("Getting SARS-CoV-2 protein summary: {Proteins}", proteinsParam);
            
            return await _httpClient.GetAsync<Sars2ProteinDatasetSummary>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SARS-CoV-2 protein summary");
            return ApiResponse<Sars2ProteinDatasetSummary>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get summary of SARS-CoV-2 protein datasets by request (POST)
    /// </summary>
    /// <param name="request">SARS-CoV-2 protein dataset request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SARS-CoV-2 protein dataset summary</returns>
    public async Task<ApiResponse<Sars2ProteinDatasetSummary>> GetSars2ProteinSummaryAsync(
        Sars2ProteinDatasetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<Sars2ProteinDatasetSummary>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/virus/taxon/sars2/protein";
            _logger.LogDebug("Getting SARS-CoV-2 protein summary via POST");
            
            return await _httpClient.PostAsync<Sars2ProteinDatasetRequest, Sars2ProteinDatasetSummary>(
                endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SARS-CoV-2 protein summary via POST");
            return ApiResponse<Sars2ProteinDatasetSummary>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Virus Genome Table

    /// <summary>
    /// Get virus genome metadata in tabular format by taxon (GET)
    /// </summary>
    /// <param name="taxon">NCBI Taxonomy ID or name</param>
    /// <param name="format">Table format (tsv, csv, jsonl)</param>
    /// <param name="refSeqOnly">Limit to RefSeq genomes only</param>
    /// <param name="completeOnly">Only include complete genomes</param>
    /// <param name="tableFields">Specify which fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tabular data as string</returns>
    public async Task<ApiResponse<string>> GetVirusGenomeTableByTaxonAsync(
        string taxon,
        string? format = null,
        bool? refSeqOnly = null,
        bool? completeOnly = null,
        IEnumerable<string>? tableFields = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taxon))
        {
            return ApiResponse<string>.Error("Taxon cannot be null or empty", 400);
        }

        try
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(format))
                queryParams.Add($"format={Uri.EscapeDataString(format)}");
            if (refSeqOnly.HasValue)
                queryParams.Add($"refseq_only={refSeqOnly.Value.ToString().ToLower()}");
            if (completeOnly.HasValue)
                queryParams.Add($"complete_only={completeOnly.Value.ToString().ToLower()}");
            if (tableFields != null)
            {
                foreach (var field in tableFields)
                    queryParams.Add($"table_fields={Uri.EscapeDataString(field)}");
            }

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/virus/taxon/{Uri.EscapeDataString(taxon)}/genome/table{query}";
            
            _logger.LogDebug("Getting virus genome table by taxon: {Taxon}", taxon);
            
            return await _httpClient.GetAsync<string>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting virus genome table by taxon");
            return ApiResponse<string>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region SARS-CoV-2 Protein Table

    /// <summary>
    /// Get SARS-CoV-2 protein metadata in tabular format (GET)
    /// </summary>
    /// <param name="proteins">List of protein names</param>
    /// <param name="format">Table format (tsv, csv, jsonl)</param>
    /// <param name="refSeqOnly">Limit to RefSeq proteins only</param>
    /// <param name="tableFields">Specify which fields to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tabular data as string</returns>
    public async Task<ApiResponse<string>> GetSars2ProteinTableAsync(
        IEnumerable<string> proteins,
        string? format = null,
        bool? refSeqOnly = null,
        IEnumerable<string>? tableFields = null,
        CancellationToken cancellationToken = default)
    {
        if (proteins == null || !proteins.Any())
        {
            return ApiResponse<string>.Error("Proteins cannot be null or empty", 400);
        }

        try
        {
            var proteinsParam = string.Join(",", proteins);
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(format))
                queryParams.Add($"format={Uri.EscapeDataString(format)}");
            if (refSeqOnly.HasValue)
                queryParams.Add($"refseq_only={refSeqOnly.Value.ToString().ToLower()}");
            if (tableFields != null)
            {
                foreach (var field in tableFields)
                    queryParams.Add($"table_fields={Uri.EscapeDataString(field)}");
            }

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/virus/taxon/sars2/protein/{proteinsParam}/table{query}";
            
            _logger.LogDebug("Getting SARS-CoV-2 protein table: {Proteins}", proteinsParam);
            
            return await _httpClient.GetAsync<string>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SARS-CoV-2 protein table");
            return ApiResponse<string>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Virus Dataset Reports

    /// <summary>
    /// Get virus metadata by taxon (GET)
    /// </summary>
    /// <param name="taxon">NCBI Taxonomy ID or name</param>
    /// <param name="returnedContent">Content type to return</param>
    /// <param name="refSeqOnly">Limit to RefSeq genomes only</param>
    /// <param name="completeOnly">Only include complete genomes</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="pageToken">Page token for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Virus data report page</returns>
    public async Task<ApiResponse<VirusDataReportPage>> GetVirusDatasetReportsByTaxonAsync(
        string taxon,
        VirusDataReportRequestContentType? returnedContent = null,
        bool? refSeqOnly = null,
        bool? completeOnly = null,
        int? pageSize = null,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taxon))
        {
            return ApiResponse<VirusDataReportPage>.Error("Taxon cannot be null or empty", 400);
        }

        try
        {
            var queryParams = new List<string>();
            
            if (returnedContent.HasValue)
                queryParams.Add($"returned_content={returnedContent.Value}");
            if (refSeqOnly.HasValue)
                queryParams.Add($"refseq_only={refSeqOnly.Value.ToString().ToLower()}");
            if (completeOnly.HasValue)
                queryParams.Add($"complete_only={completeOnly.Value.ToString().ToLower()}");
            if (pageSize.HasValue)
                queryParams.Add($"page_size={pageSize.Value}");
            if (!string.IsNullOrEmpty(pageToken))
                queryParams.Add($"page_token={Uri.EscapeDataString(pageToken)}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/virus/taxon/{Uri.EscapeDataString(taxon)}/dataset_report{query}";
            
            _logger.LogDebug("Getting virus dataset reports by taxon: {Taxon}", taxon);
            
            return await _httpClient.GetAsync<VirusDataReportPage>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting virus dataset reports by taxon");
            return ApiResponse<VirusDataReportPage>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get virus metadata by accessions (GET)
    /// </summary>
    /// <param name="accessions">List of virus genome accessions</param>
    /// <param name="returnedContent">Content type to return</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="pageToken">Page token for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Virus data report page</returns>
    public async Task<ApiResponse<VirusDataReportPage>> GetVirusDatasetReportsByAccessionAsync(
        IEnumerable<string> accessions,
        VirusDataReportRequestContentType? returnedContent = null,
        int? pageSize = null,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        if (accessions == null || !accessions.Any())
        {
            return ApiResponse<VirusDataReportPage>.Error("Accessions cannot be null or empty", 400);
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
            var endpoint = $"/virus/accession/{accessionsParam}/dataset_report{query}";
            
            _logger.LogDebug("Getting virus dataset reports by accession: {Accessions}", accessionsParam);
            
            return await _httpClient.GetAsync<VirusDataReportPage>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting virus dataset reports by accession");
            return ApiResponse<VirusDataReportPage>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get virus metadata by request (POST)
    /// </summary>
    /// <param name="request">Virus data report request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Virus data report page</returns>
    public async Task<ApiResponse<VirusDataReportPage>> GetVirusDatasetReportsAsync(
        VirusDataReportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<VirusDataReportPage>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/virus";
            _logger.LogDebug("Getting virus dataset reports via POST");
            
            return await _httpClient.PostAsync<VirusDataReportRequest, VirusDataReportPage>(
                endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting virus dataset reports via POST");
            return ApiResponse<VirusDataReportPage>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Virus Annotation Reports

    /// <summary>
    /// Get virus annotation report by taxon (GET)
    /// </summary>
    /// <param name="taxon">NCBI Taxonomy ID or name</param>
    /// <param name="refSeqOnly">Limit to RefSeq genomes only</param>
    /// <param name="completeOnly">Only include complete genomes</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="pageToken">Page token for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Virus annotation report page</returns>
    public async Task<ApiResponse<VirusAnnotationReportPage>> GetVirusAnnotationReportsByTaxonAsync(
        string taxon,
        bool? refSeqOnly = null,
        bool? completeOnly = null,
        int? pageSize = null,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taxon))
        {
            return ApiResponse<VirusAnnotationReportPage>.Error("Taxon cannot be null or empty", 400);
        }

        try
        {
            var queryParams = new List<string>();
            
            if (refSeqOnly.HasValue)
                queryParams.Add($"refseq_only={refSeqOnly.Value.ToString().ToLower()}");
            if (completeOnly.HasValue)
                queryParams.Add($"complete_only={completeOnly.Value.ToString().ToLower()}");
            if (pageSize.HasValue)
                queryParams.Add($"page_size={pageSize.Value}");
            if (!string.IsNullOrEmpty(pageToken))
                queryParams.Add($"page_token={Uri.EscapeDataString(pageToken)}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/virus/taxon/{Uri.EscapeDataString(taxon)}/annotation_report{query}";
            
            _logger.LogDebug("Getting virus annotation reports by taxon: {Taxon}", taxon);
            
            return await _httpClient.GetAsync<VirusAnnotationReportPage>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting virus annotation reports by taxon");
            return ApiResponse<VirusAnnotationReportPage>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get virus annotation report by accessions (GET)
    /// </summary>
    /// <param name="accessions">List of virus genome accessions</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="pageToken">Page token for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Virus annotation report page</returns>
    public async Task<ApiResponse<VirusAnnotationReportPage>> GetVirusAnnotationReportsByAccessionAsync(
        IEnumerable<string> accessions,
        int? pageSize = null,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        if (accessions == null || !accessions.Any())
        {
            return ApiResponse<VirusAnnotationReportPage>.Error("Accessions cannot be null or empty", 400);
        }

        try
        {
            var accessionsParam = string.Join(",", accessions);
            var queryParams = new List<string>();
            
            if (pageSize.HasValue)
                queryParams.Add($"page_size={pageSize.Value}");
            if (!string.IsNullOrEmpty(pageToken))
                queryParams.Add($"page_token={Uri.EscapeDataString(pageToken)}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/virus/accession/{accessionsParam}/annotation_report{query}";
            
            _logger.LogDebug("Getting virus annotation reports by accession: {Accessions}", accessionsParam);
            
            return await _httpClient.GetAsync<VirusAnnotationReportPage>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting virus annotation reports by accession");
            return ApiResponse<VirusAnnotationReportPage>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get virus annotation report by request (POST)
    /// </summary>
    /// <param name="request">Virus data report request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Virus annotation report page</returns>
    public async Task<ApiResponse<VirusAnnotationReportPage>> GetVirusAnnotationReportsAsync(
        VirusDataReportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<VirusAnnotationReportPage>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/virus/annotation_report";
            _logger.LogDebug("Getting virus annotation reports via POST");
            
            return await _httpClient.PostAsync<VirusDataReportRequest, VirusAnnotationReportPage>(
                endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting virus annotation reports via POST");
            return ApiResponse<VirusAnnotationReportPage>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Virus Availability Check

    /// <summary>
    /// Check virus dataset availability by accessions (GET)
    /// </summary>
    /// <param name="accessions">List of virus genome accessions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Virus availability reply</returns>
    public async Task<ApiResponse<VirusAvailabilityReply>> CheckVirusAvailabilityByAccessionAsync(
        IEnumerable<string> accessions,
        CancellationToken cancellationToken = default)
    {
        if (accessions == null || !accessions.Any())
        {
            return ApiResponse<VirusAvailabilityReply>.Error("Accessions cannot be null or empty", 400);
        }

        try
        {
            var accessionsParam = string.Join(",", accessions);
            var endpoint = $"/virus/accession/{accessionsParam}/check";
            
            _logger.LogDebug("Checking virus availability by accession: {Accessions}", accessionsParam);
            
            return await _httpClient.GetAsync<VirusAvailabilityReply>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking virus availability by accession");
            return ApiResponse<VirusAvailabilityReply>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Check virus dataset availability by request (POST)
    /// </summary>
    /// <param name="request">Virus availability request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Virus availability reply</returns>
    public async Task<ApiResponse<VirusAvailabilityReply>> CheckVirusAvailabilityAsync(
        VirusAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<VirusAvailabilityReply>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/virus/check";
            _logger.LogDebug("Checking virus availability via POST");
            
            return await _httpClient.PostAsync<VirusAvailabilityRequest, VirusAvailabilityReply>(
                endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking virus availability via POST");
            return ApiResponse<VirusAvailabilityReply>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region Virus Genome Downloads

    /// <summary>
    /// Download virus genome dataset by taxon (GET)
    /// </summary>
    /// <param name="taxon">NCBI Taxonomy ID or name</param>
    /// <param name="includeAnnotationType">Annotation types to include</param>
    /// <param name="refSeqOnly">Limit to RefSeq genomes only</param>
    /// <param name="completeOnly">Only include complete genomes</param>
    /// <param name="filename">Output filename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download stream</returns>
    public async Task<ApiResponse<byte[]>> DownloadVirusGenomeDatasetByTaxonAsync(
        string taxon,
        IEnumerable<VirusDatasetReportType>? includeAnnotationType = null,
        bool? refSeqOnly = null,
        bool? completeOnly = null,
        string? filename = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taxon))
        {
            return ApiResponse<byte[]>.Error("Taxon cannot be null or empty", 400);
        }

        try
        {
            var queryParams = new List<string>();
            
            if (includeAnnotationType != null)
            {
                foreach (var annotationType in includeAnnotationType)
                    queryParams.Add($"include_annotation_type={annotationType}");
            }
            if (refSeqOnly.HasValue)
                queryParams.Add($"refseq_only={refSeqOnly.Value.ToString().ToLower()}");
            if (completeOnly.HasValue)
                queryParams.Add($"complete_only={completeOnly.Value.ToString().ToLower()}");
            if (!string.IsNullOrEmpty(filename))
                queryParams.Add($"filename={Uri.EscapeDataString(filename)}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/virus/taxon/{Uri.EscapeDataString(taxon)}/genome/download{query}";
            
            _logger.LogDebug("Downloading virus genome dataset by taxon: {Taxon}", taxon);
            
            return await _httpClient.DownloadFileAsync(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading virus genome dataset by taxon");
            return ApiResponse<byte[]>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Download virus genome dataset by accessions (GET)
    /// </summary>
    /// <param name="accessions">List of virus genome accessions</param>
    /// <param name="includeAnnotationType">Annotation types to include</param>
    /// <param name="filename">Output filename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download stream</returns>
    public async Task<ApiResponse<byte[]>> DownloadVirusGenomeDatasetByAccessionAsync(
        IEnumerable<string> accessions,
        IEnumerable<VirusDatasetReportType>? includeAnnotationType = null,
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
            if (!string.IsNullOrEmpty(filename))
                queryParams.Add($"filename={Uri.EscapeDataString(filename)}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/virus/accession/{accessionsParam}/genome/download{query}";
            
            _logger.LogDebug("Downloading virus genome dataset by accession: {Accessions}", accessionsParam);
            
            return await _httpClient.DownloadFileAsync(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading virus genome dataset by accession");
            return ApiResponse<byte[]>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Download virus genome dataset by request (POST)
    /// </summary>
    /// <param name="request">Virus dataset request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download stream</returns>
    public async Task<ApiResponse<byte[]>> DownloadVirusGenomeDatasetAsync(
        VirusDatasetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<byte[]>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/virus/genome/download";
            _logger.LogDebug("Downloading virus genome dataset via POST");
            
            return await _httpClient.PostDownloadAsync(endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading virus genome dataset via POST");
            return ApiResponse<byte[]>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion

    #region SARS-CoV-2 Protein Downloads

    /// <summary>
    /// Download SARS-CoV-2 protein datasets by protein name (GET)
    /// </summary>
    /// <param name="proteins">List of protein names</param>
    /// <param name="includeAnnotationType">Annotation types to include</param>
    /// <param name="refSeqOnly">Limit to RefSeq proteins only</param>
    /// <param name="filename">Output filename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download stream</returns>
    public async Task<ApiResponse<byte[]>> DownloadSars2ProteinDatasetAsync(
        IEnumerable<string> proteins,
        IEnumerable<VirusDatasetReportType>? includeAnnotationType = null,
        bool? refSeqOnly = null,
        string? filename = null,
        CancellationToken cancellationToken = default)
    {
        if (proteins == null || !proteins.Any())
        {
            return ApiResponse<byte[]>.Error("Proteins cannot be null or empty", 400);
        }

        try
        {
            var proteinsParam = string.Join(",", proteins);
            var queryParams = new List<string>();
            
            if (includeAnnotationType != null)
            {
                foreach (var annotationType in includeAnnotationType)
                    queryParams.Add($"include_annotation_type={annotationType}");
            }
            if (refSeqOnly.HasValue)
                queryParams.Add($"refseq_only={refSeqOnly.Value.ToString().ToLower()}");
            if (!string.IsNullOrEmpty(filename))
                queryParams.Add($"filename={Uri.EscapeDataString(filename)}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var endpoint = $"/virus/taxon/sars2/protein/{proteinsParam}/download{query}";
            
            _logger.LogDebug("Downloading SARS-CoV-2 protein dataset: {Proteins}", proteinsParam);
            
            return await _httpClient.DownloadFileAsync(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading SARS-CoV-2 protein dataset");
            return ApiResponse<byte[]>.Error($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Download SARS-CoV-2 protein datasets by request (POST)
    /// </summary>
    /// <param name="request">SARS-CoV-2 protein dataset request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download stream</returns>
    public async Task<ApiResponse<byte[]>> DownloadSars2ProteinDatasetAsync(
        Sars2ProteinDatasetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return ApiResponse<byte[]>.Error("Request cannot be null", 400);
        }

        try
        {
            const string endpoint = "/virus/taxon/sars2/protein/download";
            _logger.LogDebug("Downloading SARS-CoV-2 protein dataset via POST");
            
            return await _httpClient.PostDownloadAsync(endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading SARS-CoV-2 protein dataset via POST");
            return ApiResponse<byte[]>.Error($"Unexpected error: {ex.Message}");
        }
    }

    #endregion
}
