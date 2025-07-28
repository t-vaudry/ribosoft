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
    /// Service for interacting with NCBI Datasets Taxonomy API endpoints
    /// </summary>
    public class TaxonomyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        /// <summary>
        /// Initializes a new instance of the TaxonomyService
        /// </summary>
        /// <param name="httpClient">HTTP client for making requests</param>
        /// <param name="baseUrl">Base URL for the NCBI Datasets API</param>
        public TaxonomyService(HttpClient httpClient, string baseUrl = "https://api.ncbi.nlm.nih.gov/datasets/v2")
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        #region Taxonomy Metadata Operations

        /// <summary>
        /// Get taxonomy metadata by taxon identifiers (GET)
        /// </summary>
        /// <param name="taxons">List of taxon identifiers (tax IDs or names)</param>
        /// <param name="returnedContent">Type of content to return</param>
        /// <param name="pageSize">Maximum number of results to return</param>
        /// <param name="pageToken">Page token for pagination</param>
        /// <param name="tableFormat">Format for tabular data</param>
        /// <param name="children">Include children in taxonomy explosion</param>
        /// <param name="ranks">Taxonomic ranks to filter by</param>
        /// <returns>Taxonomy metadata response</returns>
        public async Task<ApiResponse<TaxonomyMetadataResponse>> GetTaxonomyMetadataByTaxonAsync(
            List<string> taxons,
            TaxonomyMetadataRequestContentType? returnedContent = null,
            int? pageSize = null,
            string? pageToken = null,
            TaxonomyMetadataRequestTableFormat? tableFormat = null,
            bool? children = null,
            List<RankType>? ranks = null)
        {
            if (taxons == null || !taxons.Any())
                return ApiResponse<TaxonomyMetadataResponse>.Error("Taxons parameter is required and cannot be empty");

            try
            {
                var taxonString = string.Join(",", taxons.Select(HttpUtility.UrlEncode));
                var queryParams = new List<string>();

                if (returnedContent.HasValue)
                    queryParams.Add($"returned_content={returnedContent.Value}");
                if (pageSize.HasValue)
                    queryParams.Add($"page_size={pageSize.Value}");
                if (!string.IsNullOrEmpty(pageToken))
                    queryParams.Add($"page_token={HttpUtility.UrlEncode(pageToken)}");
                if (tableFormat.HasValue)
                    queryParams.Add($"table_format={tableFormat.Value}");
                if (children.HasValue)
                    queryParams.Add($"children={children.Value.ToString().ToLower()}");
                if (ranks != null && ranks.Any())
                    queryParams.Add($"ranks={string.Join(",", ranks)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_baseUrl}/taxonomy/taxon/{taxonString}{queryString}";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TaxonomyMetadataResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<TaxonomyMetadataResponse>.Success(result!);
                }

                return ApiResponse<TaxonomyMetadataResponse>.Error($"API request failed: {response.StatusCode} - {content}");
            }
            catch (Exception ex)
            {
                return ApiResponse<TaxonomyMetadataResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get taxonomy metadata using POST request
        /// </summary>
        /// <param name="request">Taxonomy metadata request</param>
        /// <returns>Taxonomy metadata response</returns>
        public async Task<ApiResponse<TaxonomyMetadataResponse>> GetTaxonomyMetadataAsync(TaxonomyMetadataRequest request)
        {
            if (request == null)
                return ApiResponse<TaxonomyMetadataResponse>.Error("Request parameter is required");

            if (request.Taxons == null || !request.Taxons.Any())
                return ApiResponse<TaxonomyMetadataResponse>.Error("Taxons are required");

            try
            {
                var url = $"{_baseUrl}/taxonomy";
                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TaxonomyMetadataResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<TaxonomyMetadataResponse>.Success(result!);
                }

                return ApiResponse<TaxonomyMetadataResponse>.Error($"API request failed: {response.StatusCode} - {responseContent}");
            }
            catch (Exception ex)
            {
                return ApiResponse<TaxonomyMetadataResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        #endregion

        #region Taxonomy Dataset Reports

        /// <summary>
        /// Get taxonomy dataset reports by taxon identifiers (GET)
        /// </summary>
        /// <param name="taxons">List of taxon identifiers</param>
        /// <param name="returnedContent">Type of content to return</param>
        /// <param name="pageSize">Maximum number of results to return</param>
        /// <param name="pageToken">Page token for pagination</param>
        /// <param name="tableFormat">Format for tabular data</param>
        /// <param name="children">Include children in taxonomy explosion</param>
        /// <param name="ranks">Taxonomic ranks to filter by</param>
        /// <returns>Taxonomy metadata response</returns>
        public async Task<ApiResponse<TaxonomyMetadataResponse>> GetTaxonomyDatasetReportsByTaxonAsync(
            List<string> taxons,
            TaxonomyMetadataRequestContentType? returnedContent = null,
            int? pageSize = null,
            string? pageToken = null,
            TaxonomyMetadataRequestTableFormat? tableFormat = null,
            bool? children = null,
            List<RankType>? ranks = null)
        {
            if (taxons == null || !taxons.Any())
                return ApiResponse<TaxonomyMetadataResponse>.Error("Taxons parameter is required and cannot be empty");

            try
            {
                var taxonString = string.Join(",", taxons.Select(HttpUtility.UrlEncode));
                var queryParams = new List<string>();

                if (returnedContent.HasValue)
                    queryParams.Add($"returned_content={returnedContent.Value}");
                if (pageSize.HasValue)
                    queryParams.Add($"page_size={pageSize.Value}");
                if (!string.IsNullOrEmpty(pageToken))
                    queryParams.Add($"page_token={HttpUtility.UrlEncode(pageToken)}");
                if (tableFormat.HasValue)
                    queryParams.Add($"table_format={tableFormat.Value}");
                if (children.HasValue)
                    queryParams.Add($"children={children.Value.ToString().ToLower()}");
                if (ranks != null && ranks.Any())
                    queryParams.Add($"ranks={string.Join(",", ranks)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_baseUrl}/taxonomy/taxon/{taxonString}/dataset_report{queryString}";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TaxonomyMetadataResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<TaxonomyMetadataResponse>.Success(result!);
                }

                return ApiResponse<TaxonomyMetadataResponse>.Error($"API request failed: {response.StatusCode} - {content}");
            }
            catch (Exception ex)
            {
                return ApiResponse<TaxonomyMetadataResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get taxonomy dataset reports using POST request
        /// </summary>
        /// <param name="request">Taxonomy metadata request</param>
        /// <returns>Taxonomy metadata response</returns>
        public async Task<ApiResponse<TaxonomyMetadataResponse>> GetTaxonomyDatasetReportsAsync(TaxonomyMetadataRequest request)
        {
            if (request == null)
                return ApiResponse<TaxonomyMetadataResponse>.Error("Request parameter is required");

            if (request.Taxons == null || !request.Taxons.Any())
                return ApiResponse<TaxonomyMetadataResponse>.Error("Taxons are required");

            try
            {
                var url = $"{_baseUrl}/taxonomy/dataset_report";
                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TaxonomyMetadataResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<TaxonomyMetadataResponse>.Success(result!);
                }

                return ApiResponse<TaxonomyMetadataResponse>.Error($"API request failed: {response.StatusCode} - {responseContent}");
            }
            catch (Exception ex)
            {
                return ApiResponse<TaxonomyMetadataResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        #endregion

        #region Taxonomy Name Reports

        /// <summary>
        /// Get taxonomy name reports by taxon identifiers (GET)
        /// </summary>
        /// <param name="taxons">List of taxon identifiers</param>
        /// <param name="returnedContent">Type of content to return</param>
        /// <param name="pageSize">Maximum number of results to return</param>
        /// <param name="pageToken">Page token for pagination</param>
        /// <param name="tableFormat">Format for tabular data</param>
        /// <param name="children">Include children in taxonomy explosion</param>
        /// <param name="ranks">Taxonomic ranks to filter by</param>
        /// <returns>Taxonomy metadata response</returns>
        public async Task<ApiResponse<TaxonomyMetadataResponse>> GetTaxonomyNameReportsByTaxonAsync(
            List<string> taxons,
            TaxonomyMetadataRequestContentType? returnedContent = null,
            int? pageSize = null,
            string? pageToken = null,
            TaxonomyMetadataRequestTableFormat? tableFormat = null,
            bool? children = null,
            List<RankType>? ranks = null)
        {
            if (taxons == null || !taxons.Any())
                return ApiResponse<TaxonomyMetadataResponse>.Error("Taxons parameter is required and cannot be empty");

            try
            {
                var taxonString = string.Join(",", taxons.Select(HttpUtility.UrlEncode));
                var queryParams = new List<string>();

                if (returnedContent.HasValue)
                    queryParams.Add($"returned_content={returnedContent.Value}");
                if (pageSize.HasValue)
                    queryParams.Add($"page_size={pageSize.Value}");
                if (!string.IsNullOrEmpty(pageToken))
                    queryParams.Add($"page_token={HttpUtility.UrlEncode(pageToken)}");
                if (tableFormat.HasValue)
                    queryParams.Add($"table_format={tableFormat.Value}");
                if (children.HasValue)
                    queryParams.Add($"children={children.Value.ToString().ToLower()}");
                if (ranks != null && ranks.Any())
                    queryParams.Add($"ranks={string.Join(",", ranks)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_baseUrl}/taxonomy/taxon/{taxonString}/name_report{queryString}";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TaxonomyMetadataResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<TaxonomyMetadataResponse>.Success(result!);
                }

                return ApiResponse<TaxonomyMetadataResponse>.Error($"API request failed: {response.StatusCode} - {content}");
            }
            catch (Exception ex)
            {
                return ApiResponse<TaxonomyMetadataResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get taxonomy name reports using POST request
        /// </summary>
        /// <param name="request">Taxonomy metadata request</param>
        /// <returns>Taxonomy metadata response</returns>
        public async Task<ApiResponse<TaxonomyMetadataResponse>> GetTaxonomyNameReportsAsync(TaxonomyMetadataRequest request)
        {
            if (request == null)
                return ApiResponse<TaxonomyMetadataResponse>.Error("Request parameter is required");

            if (request.Taxons == null || !request.Taxons.Any())
                return ApiResponse<TaxonomyMetadataResponse>.Error("Taxons are required");

            try
            {
                var url = $"{_baseUrl}/taxonomy/name_report";
                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TaxonomyMetadataResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<TaxonomyMetadataResponse>.Success(result!);
                }

                return ApiResponse<TaxonomyMetadataResponse>.Error($"API request failed: {response.StatusCode} - {responseContent}");
            }
            catch (Exception ex)
            {
                return ApiResponse<TaxonomyMetadataResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        #endregion

        #region Taxonomy Related IDs

        /// <summary>
        /// Get related taxonomy IDs by taxonomy ID (GET)
        /// </summary>
        /// <param name="taxId">Taxonomy ID to find related IDs for</param>
        /// <param name="relationshipTypes">Types of relationships to include</param>
        /// <returns>Taxonomy related ID response</returns>
        public async Task<ApiResponse<TaxonomyRelatedIdResponse>> GetTaxonomyRelatedIdsByTaxIdAsync(
            int taxId,
            List<TaxonomyRelationshipType>? relationshipTypes = null)
        {
            try
            {
                var queryParams = new List<string>();

                if (relationshipTypes != null && relationshipTypes.Any())
                    queryParams.Add($"relationship_types={string.Join(",", relationshipTypes)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_baseUrl}/taxonomy/taxon/{taxId}/related_ids{queryString}";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TaxonomyRelatedIdResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<TaxonomyRelatedIdResponse>.Success(result!);
                }

                return ApiResponse<TaxonomyRelatedIdResponse>.Error($"API request failed: {response.StatusCode} - {content}");
            }
            catch (Exception ex)
            {
                return ApiResponse<TaxonomyRelatedIdResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get related taxonomy IDs using POST request
        /// </summary>
        /// <param name="request">Taxonomy related ID request</param>
        /// <returns>Taxonomy related ID response</returns>
        public async Task<ApiResponse<TaxonomyRelatedIdResponse>> GetTaxonomyRelatedIdsAsync(TaxonomyRelatedIdRequest request)
        {
            if (request == null)
                return ApiResponse<TaxonomyRelatedIdResponse>.Error("Request parameter is required");

            try
            {
                var url = $"{_baseUrl}/taxonomy/related_ids";
                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TaxonomyRelatedIdResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<TaxonomyRelatedIdResponse>.Success(result!);
                }

                return ApiResponse<TaxonomyRelatedIdResponse>.Error($"API request failed: {response.StatusCode} - {responseContent}");
            }
            catch (Exception ex)
            {
                return ApiResponse<TaxonomyRelatedIdResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        #endregion

        #region Taxonomy Suggestions

        /// <summary>
        /// Get taxonomy suggestions by query string (GET)
        /// </summary>
        /// <param name="taxonQuery">Partial taxonomic name to search for</param>
        /// <param name="limit">Maximum number of suggestions to return</param>
        /// <param name="ranks">Taxonomic ranks to filter suggestions</param>
        /// <returns>Taxonomy suggestion response</returns>
        public async Task<ApiResponse<TaxonomySuggestionResponse>> GetTaxonomySuggestionsByQueryAsync(
            string taxonQuery,
            int? limit = null,
            List<RankType>? ranks = null)
        {
            if (string.IsNullOrEmpty(taxonQuery))
                return ApiResponse<TaxonomySuggestionResponse>.Error("Taxon query parameter is required");

            try
            {
                var queryParams = new List<string>();

                if (limit.HasValue)
                    queryParams.Add($"limit={limit.Value}");
                if (ranks != null && ranks.Any())
                    queryParams.Add($"ranks={string.Join(",", ranks)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_baseUrl}/taxonomy/taxon_suggest/{HttpUtility.UrlEncode(taxonQuery)}{queryString}";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TaxonomySuggestionResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<TaxonomySuggestionResponse>.Success(result!);
                }

                return ApiResponse<TaxonomySuggestionResponse>.Error($"API request failed: {response.StatusCode} - {content}");
            }
            catch (Exception ex)
            {
                return ApiResponse<TaxonomySuggestionResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get taxonomy suggestions using POST request
        /// </summary>
        /// <param name="request">Taxonomy suggestion request</param>
        /// <returns>Taxonomy suggestion response</returns>
        public async Task<ApiResponse<TaxonomySuggestionResponse>> GetTaxonomySuggestionsAsync(TaxonomySuggestionRequest request)
        {
            if (request == null)
                return ApiResponse<TaxonomySuggestionResponse>.Error("Request parameter is required");

            if (string.IsNullOrEmpty(request.TaxonQuery))
                return ApiResponse<TaxonomySuggestionResponse>.Error("Taxon query is required");

            try
            {
                var url = $"{_baseUrl}/taxonomy/taxon_suggest";
                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TaxonomySuggestionResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<TaxonomySuggestionResponse>.Success(result!);
                }

                return ApiResponse<TaxonomySuggestionResponse>.Error($"API request failed: {response.StatusCode} - {responseContent}");
            }
            catch (Exception ex)
            {
                return ApiResponse<TaxonomySuggestionResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        #endregion

        #region Taxonomy Downloads

        /// <summary>
        /// Download taxonomy dataset by taxonomy IDs (GET)
        /// </summary>
        /// <param name="taxIds">List of taxonomy IDs</param>
        /// <param name="auxReports">Additional reports to include</param>
        /// <returns>Binary data stream</returns>
        public async Task<ApiResponse<byte[]>> DownloadTaxonomyDatasetByTaxIdAsync(
            List<int> taxIds,
            List<TaxonomyDatasetRequestTaxonomyReportType>? auxReports = null)
        {
            if (taxIds == null || !taxIds.Any())
                return ApiResponse<byte[]>.Error("Tax IDs parameter is required and cannot be empty");

            try
            {
                var taxIdString = string.Join(",", taxIds);
                var queryParams = new List<string>();

                if (auxReports != null && auxReports.Any())
                    queryParams.Add($"aux_reports={string.Join(",", auxReports)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_baseUrl}/taxonomy/taxon/{taxIdString}/download{queryString}";

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
        /// Download taxonomy dataset using POST request
        /// </summary>
        /// <param name="request">Taxonomy dataset request</param>
        /// <returns>Binary data stream</returns>
        public async Task<ApiResponse<byte[]>> DownloadTaxonomyDatasetAsync(TaxonomyDatasetRequest request)
        {
            if (request == null)
                return ApiResponse<byte[]>.Error("Request parameter is required");

            if (request.TaxIds == null || !request.TaxIds.Any())
                return ApiResponse<byte[]>.Error("Tax IDs are required");

            try
            {
                var url = $"{_baseUrl}/taxonomy/download";
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
    }
}
