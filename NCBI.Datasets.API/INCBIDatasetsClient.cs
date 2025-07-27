using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Download;
using NCBI.Datasets.API.Models.Genome;

namespace NCBI.Datasets.API;

/// <summary>
/// Interface for NCBI Datasets API client
/// </summary>
public interface INCBIDatasetsClient : IDisposable
{
    /// <summary>
    /// Gets download summary for genome assemblies
    /// </summary>
    /// <param name="request">Download summary request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download summary response</returns>
    Task<ApiResponse<DownloadSummary>> GetGenomeDownloadSummaryAsync(
        GenomeDownloadSummaryRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets download summary for genome assemblies using POST method
    /// </summary>
    /// <param name="request">Download summary request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download summary response</returns>
    Task<ApiResponse<DownloadSummary>> GetGenomeDownloadSummaryByPostAsync(
        GenomeDownloadSummaryRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads genome dataset from URL
    /// </summary>
    /// <param name="downloadUrl">Download URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Downloaded file content</returns>
    Task<ApiResponse<byte[]>> DownloadGenomeDatasetAsync(
        string downloadUrl, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets download summary for a single genome assembly accession
    /// </summary>
    /// <param name="accession">Assembly accession</param>
    /// <param name="chromosomes">Optional chromosomes to include</param>
    /// <param name="annotationTypes">Optional annotation types to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download summary response</returns>
    Task<ApiResponse<DownloadSummary>> GetGenomeDownloadSummaryAsync(
        string accession,
        IEnumerable<string>? chromosomes = null,
        IEnumerable<Models.Enums.AnnotationForAssemblyType>? annotationTypes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets download summary for multiple genome assembly accessions
    /// </summary>
    /// <param name="accessions">Assembly accessions</param>
    /// <param name="chromosomes">Optional chromosomes to include</param>
    /// <param name="annotationTypes">Optional annotation types to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download summary response</returns>
    Task<ApiResponse<DownloadSummary>> GetGenomeDownloadSummaryAsync(
        IEnumerable<string> accessions,
        IEnumerable<string>? chromosomes = null,
        IEnumerable<Models.Enums.AnnotationForAssemblyType>? annotationTypes = null,
        CancellationToken cancellationToken = default);
}
