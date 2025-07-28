using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Requests;
using NCBI.Datasets.API.Models.Responses;

namespace NCBI.Datasets.API.Services
{
    /// <summary>
    /// Service for interacting with NCBI Datasets BioSample API endpoints
    /// </summary>
    public class BioSampleService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        /// <summary>
        /// Initializes a new instance of the BioSampleService
        /// </summary>
        /// <param name="httpClient">HTTP client for making requests</param>
        /// <param name="baseUrl">Base URL for the NCBI Datasets API</param>
        public BioSampleService(HttpClient httpClient, string baseUrl = "https://api.ncbi.nlm.nih.gov/datasets/v2")
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        #region BioSample Dataset Reports

        /// <summary>
        /// Get BioSample dataset reports by accessions
        /// </summary>
        /// <param name="accessions">List of BioSample accessions</param>
        /// <param name="pageSize">Maximum number of results to return (default: 20, max: 1000)</param>
        /// <param name="pageToken">Page token for pagination</param>
        /// <param name="includeTabularHeader">Whether to include tabular header row</param>
        /// <returns>BioSample data report response</returns>
        public async Task<ApiResponse<BioSampleDataReportResponse>> GetBioSampleDatasetReportsByAccessionAsync(
            List<string> accessions,
            int? pageSize = null,
            string? pageToken = null,
            bool? includeTabularHeader = null)
        {
            if (accessions == null || !accessions.Any())
                return ApiResponse<BioSampleDataReportResponse>.Error("Accessions parameter is required and cannot be empty");

            try
            {
                var accessionsString = string.Join(",", accessions.Select(HttpUtility.UrlEncode));
                var queryParams = new List<string>();

                if (pageSize.HasValue)
                {
                    if (pageSize.Value > 1000)
                        return ApiResponse<BioSampleDataReportResponse>.Error("Page size cannot exceed 1000");
                    queryParams.Add($"page_size={pageSize.Value}");
                }

                if (!string.IsNullOrEmpty(pageToken))
                    queryParams.Add($"page_token={HttpUtility.UrlEncode(pageToken)}");
                if (includeTabularHeader.HasValue)
                    queryParams.Add($"include_tabular_header={includeTabularHeader.Value.ToString().ToLower()}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_baseUrl}/biosample/accession/{accessionsString}/biosample_report{queryString}";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<BioSampleDataReportResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });
                    return ApiResponse<BioSampleDataReportResponse>.Success(result!);
                }

                return ApiResponse<BioSampleDataReportResponse>.Error($"API request failed: {response.StatusCode} - {content}");
            }
            catch (Exception ex)
            {
                return ApiResponse<BioSampleDataReportResponse>.Error($"Request failed: {ex.Message}");
            }
        }

        #endregion
    }
}
