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
    /// Service for interacting with NCBI Datasets Protein API endpoints
    /// </summary>
    public class ProteinService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        /// <summary>
        /// Initializes a new instance of the ProteinService
        /// </summary>
        /// <param name="httpClient">HTTP client for making requests</param>
        /// <param name="baseUrl">Base URL for the NCBI Datasets API</param>
        public ProteinService(HttpClient httpClient, string baseUrl = "https://api.ncbi.nlm.nih.gov/datasets/v2")
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        #region Prokaryote Gene Download Operations

        /// <summary>
        /// Download prokaryote gene dataset by RefSeq protein accessions (GET)
        /// </summary>
        /// <param name="accessions">List of RefSeq protein accessions (WP_ prefixed)</param>
        /// <param name="includeAnnotationType">Annotation types to include</param>
        /// <param name="geneFlankLength">Length of gene flanking sequences</param>
        /// <param name="taxon">NCBI Taxonomy ID or name to filter results</param>
        /// <param name="filename">Output filename</param>
        /// <returns>Binary ZIP file data</returns>
        public async Task<ApiResponse<byte[]>> DownloadProkaryoteGeneDatasetByAccessionAsync(
            List<string> accessions,
            List<FastaType>? includeAnnotationType = null,
            int? geneFlankLength = null,
            string? taxon = null,
            string? filename = null)
        {
            if (accessions == null || !accessions.Any())
                return ApiResponse<byte[]>.Error("Accessions parameter is required and cannot be empty");

            try
            {
                var accessionsString = string.Join(",", accessions.Select(HttpUtility.UrlEncode));
                var queryParams = new List<string>();

                if (includeAnnotationType != null && includeAnnotationType.Any())
                    queryParams.Add($"include_annotation_type={string.Join(",", includeAnnotationType)}");
                if (geneFlankLength.HasValue)
                    queryParams.Add($"gene_flank_length={geneFlankLength.Value}");
                if (!string.IsNullOrEmpty(taxon))
                    queryParams.Add($"taxon={HttpUtility.UrlEncode(taxon)}");
                if (!string.IsNullOrEmpty(filename))
                    queryParams.Add($"filename={HttpUtility.UrlEncode(filename)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_baseUrl}/protein/accession/{accessionsString}/download{queryString}";

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
        /// Download prokaryote gene dataset by RefSeq protein accessions using POST request
        /// </summary>
        /// <param name="request">Prokaryote gene request</param>
        /// <param name="filename">Output filename</param>
        /// <returns>Binary ZIP file data</returns>
        public async Task<ApiResponse<byte[]>> DownloadProkaryoteGeneDatasetAsync(
            ProkaryoteGeneRequest request,
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
                var url = $"{_baseUrl}/protein/accession/download{queryString}";

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
