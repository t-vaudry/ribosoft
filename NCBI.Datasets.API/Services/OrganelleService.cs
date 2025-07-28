using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Enums;
using NCBI.Datasets.API.Models.Requests;
using NCBI.Datasets.API.Models.Responses;

namespace NCBI.Datasets.API.Services
{
    /// <summary>
    /// Service for interacting with NCBI Datasets Organelle API endpoints
    /// </summary>
    public class OrganelleService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        /// <summary>
        /// Initializes a new instance of the OrganelleService
        /// </summary>
        /// <param name="httpClient">HTTP client for making requests</param>
        /// <param name="baseUrl">Base URL for the NCBI Datasets API</param>
        public OrganelleService(HttpClient httpClient, string baseUrl = "https://api.ncbi.nlm.nih.gov/datasets/v2")
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        #region Organelle Download Operations

        /// <summary>
        /// Download organelle data package by accessions (GET)
        /// </summary>
        /// <param name="accessions">List of organelle accessions</param>
        /// <param name="excludeSequence">Set to true to omit genomic sequence</param>
        /// <param name="includeAnnotationType">Annotation types to include</param>
        /// <param name="hydration">Hydration level for the dataset</param>
        /// <param name="filename">Output filename</param>
        /// <returns>Binary ZIP file data</returns>
        public async Task<ApiResponse<byte[]>> DownloadOrganelleDatasetByAccessionAsync(
            List<string> accessions,
            bool? excludeSequence = null,
            List<AnnotationForOrganelleType>? includeAnnotationType = null,
            HydrationLevel? hydration = null,
            string? filename = null)
        {
            if (accessions == null || !accessions.Any())
                return ApiResponse<byte[]>.Error("Accessions parameter is required and cannot be empty");

            try
            {
                var accessionsString = string.Join(",", accessions.Select(HttpUtility.UrlEncode));
                var queryParams = new List<string>();

                if (excludeSequence.HasValue)
                    queryParams.Add($"exclude_sequence={excludeSequence.Value.ToString().ToLower()}");
                if (includeAnnotationType != null && includeAnnotationType.Any())
                    queryParams.Add($"include_annotation_type={string.Join(",", includeAnnotationType)}");
                if (hydration.HasValue)
                    queryParams.Add($"hydration={hydration.Value}");
                if (!string.IsNullOrEmpty(filename))
                    queryParams.Add($"filename={HttpUtility.UrlEncode(filename)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_baseUrl}/organelle/accession/{accessionsString}/download{queryString}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsByteArrayAsync();
                    return ApiResponse<byte[]>.Success(data);
                }

                var content = await response.Content.ReadAsStringAsync();
                return ApiResponse<byte[]>.Error($"API request failed: {response.StatusCode} - {content}");
            }
            catch (Exception ex)
            {
                return ApiResponse<byte[]>.Error($"Request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Download organelle data package using POST request
        /// </summary>
        /// <param name="request">Organelle download request</param>
        /// <param name="filename">Output filename</param>
        /// <returns>Binary ZIP file data</returns>
        public async Task<ApiResponse<byte[]>> DownloadOrganelleDatasetAsync(
            OrganelleDownloadRequest request,
            string? filename = null)
        {
            if (request == null)
                return ApiResponse<byte[]>.Error("Request parameter is required");

            if (request.Accessions == null || !request.Accessions.Any())
                return ApiResponse<byte[]>.Error("Accessions are required");

            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filename))
                    queryParams.Add($"filename={HttpUtility.UrlEncode(filename)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_baseUrl}/organelle/download{queryString}";

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsByteArrayAsync();
                    return ApiResponse<byte[]>.Success(data);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return ApiResponse<byte[]>.Error($"API request failed: {response.StatusCode} - {responseContent}");
            }
            catch (Exception ex)
            {
                return ApiResponse<byte[]>.Error($"Request failed: {ex.Message}");
            }
        }

        #endregion

        #region Organelle Dataset Reports

        /// <summary>
        /// Get organelle dataset report by accessions (GET)
        /// </summary>
        /// <param name="accessions">List of organelle accessions</param>
        /// <param name="pageSize">Maximum number of results to return</param>
        /// <param name="pageToken">Page token for pagination</param>
        /// <param name="includeTabularHeader">Whether to include tabular header</param>
        /// <param name="returnedContent">Type of content to return</param>
        /// <returns>Organelle data reports response</returns>
        public async Task<ApiResponse<OrganelleDataReportsResponse>> GetOrganelleDatasetReportsByAccessionAsync(
            List<string> accessions,
            int? pageSize = null,
            string? pageToken = null,
            bool? includeTabularHeader = null,
            OrganelleMetadataRequestContentType? returnedContent = null)
        {
            if (accessions == null || !accessions.Any())
                return ApiResponse<OrganelleDataReportsResponse>.Error("Accessions parameter is required and cannot be empty");

            try
            {
                var accessionsString = string.Join(",", accessions.Select(HttpUtility.UrlEncode));
                var queryParams = new List<string>();

                if (pageSize.HasValue)
                    queryParams.Add($"page_size={pageSize.Value}");
                if (!string.IsNullOrEmpty(pageToken))
                    queryParams.Add($"page_token={HttpUtility.UrlEncode(pageToken)}");
                if (includeTabularHeader.HasValue)
                    queryParams.Add($"include_tabular_header={includeTabularHeader.Value.ToString().ToLower()}");
                if (returnedContent.HasValue)
                    queryParams.Add($"returned_content={returnedContent.Value}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_baseUrl}/organelle/accessions/{accessionsString}/dataset_report{queryString}";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<OrganelleDataReportsResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<OrganelleDataReportsResponse>.Success(result!);
                }

                return ApiResponse<OrganelleDataReportsResponse>.Error($"API request failed: {response.StatusCode} - {content}");
            }
            catch (Exception ex)
            {
                return ApiResponse<OrganelleDataReportsResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get organelle dataset report by taxons (GET)
        /// </summary>
        /// <param name="taxons">List of taxon identifiers</param>
        /// <param name="pageSize">Maximum number of results to return</param>
        /// <param name="pageToken">Page token for pagination</param>
        /// <param name="includeTabularHeader">Whether to include tabular header</param>
        /// <param name="returnedContent">Type of content to return</param>
        /// <returns>Organelle data reports response</returns>
        public async Task<ApiResponse<OrganelleDataReportsResponse>> GetOrganelleDatasetReportsByTaxonAsync(
            List<string> taxons,
            int? pageSize = null,
            string? pageToken = null,
            bool? includeTabularHeader = null,
            OrganelleMetadataRequestContentType? returnedContent = null)
        {
            if (taxons == null || !taxons.Any())
                return ApiResponse<OrganelleDataReportsResponse>.Error("Taxons parameter is required and cannot be empty");

            try
            {
                var taxonsString = string.Join(",", taxons.Select(HttpUtility.UrlEncode));
                var queryParams = new List<string>();

                if (pageSize.HasValue)
                    queryParams.Add($"page_size={pageSize.Value}");
                if (!string.IsNullOrEmpty(pageToken))
                    queryParams.Add($"page_token={HttpUtility.UrlEncode(pageToken)}");
                if (includeTabularHeader.HasValue)
                    queryParams.Add($"include_tabular_header={includeTabularHeader.Value.ToString().ToLower()}");
                if (returnedContent.HasValue)
                    queryParams.Add($"returned_content={returnedContent.Value}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_baseUrl}/organelle/taxon/{taxonsString}/dataset_report{queryString}";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<OrganelleDataReportsResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<OrganelleDataReportsResponse>.Success(result!);
                }

                return ApiResponse<OrganelleDataReportsResponse>.Error($"API request failed: {response.StatusCode} - {content}");
            }
            catch (Exception ex)
            {
                return ApiResponse<OrganelleDataReportsResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get organelle dataset reports using POST request
        /// </summary>
        /// <param name="request">Organelle metadata request</param>
        /// <returns>Organelle data reports response</returns>
        public async Task<ApiResponse<OrganelleDataReportsResponse>> GetOrganelleDatasetReportsAsync(
            OrganelleMetadataRequest request)
        {
            if (request == null)
                return ApiResponse<OrganelleDataReportsResponse>.Error("Request parameter is required");

            if ((request.Taxons == null || !request.Taxons.Any()) && 
                (request.Accessions == null || !request.Accessions.Any()))
                return ApiResponse<OrganelleDataReportsResponse>.Error("Either taxons or accessions are required");

            try
            {
                var url = $"{_baseUrl}/organelle/dataset_report";
                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<OrganelleDataReportsResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<OrganelleDataReportsResponse>.Success(result!);
                }

                return ApiResponse<OrganelleDataReportsResponse>.Error($"API request failed: {response.StatusCode} - {responseContent}");
            }
            catch (Exception ex)
            {
                return ApiResponse<OrganelleDataReportsResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        #endregion
    }
}
