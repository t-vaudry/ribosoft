using NCBI.Datasets.API.Models.Common;

namespace NCBI.Datasets.API.Services;

/// <summary>
/// Interface for NCBI Datasets HTTP client
/// </summary>
public interface INCBIDatasetsHttpClient : IDisposable
{
    /// <summary>
    /// Performs a GET request with retry logic
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a POST request with retry logic
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="request">Request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from the specified URL
    /// </summary>
    /// <param name="url">Download URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File content as byte array</returns>
    Task<ApiResponse<byte[]>> DownloadFileAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file using POST request with request body
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="request">Request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File content as byte array</returns>
    Task<ApiResponse<byte[]>> PostDownloadAsync<TRequest>(
        string endpoint, 
        TRequest request, 
        CancellationToken cancellationToken = default);
}
