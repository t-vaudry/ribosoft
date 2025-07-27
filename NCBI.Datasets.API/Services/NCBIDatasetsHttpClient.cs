using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCBI.Datasets.API.Configuration;
using NCBI.Datasets.API.Models.Common;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NCBI.Datasets.API.Services;

/// <summary>
/// HTTP client service for NCBI Datasets API
/// </summary>
public class NCBIDatasetsHttpClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly NCBIDatasetsApiOptions _options;
    private readonly ILogger<NCBIDatasetsHttpClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the NCBIDatasetsHttpClient
    /// </summary>
    /// <param name="httpClient">HTTP client instance</param>
    /// <param name="options">API configuration options</param>
    /// <param name="logger">Logger instance</param>
    public NCBIDatasetsHttpClient(
        HttpClient httpClient,
        IOptions<NCBIDatasetsApiOptions> options,
        ILogger<NCBIDatasetsHttpClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        ConfigureHttpClient();
    }

    /// <summary>
    /// Configures the HTTP client with base settings
    /// </summary>
    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        
        // Add API key header
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("api-key", _options.ApiKey);
        }

        // Add common headers
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "NCBI.Datasets.API.Client/1.0.0");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    /// <summary>
    /// Performs a GET request with retry logic
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            _logger.LogDebug("Making GET request to: {Endpoint}", endpoint);
            
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            return await ProcessResponseAsync<T>(response);
        });
    }

    /// <summary>
    /// Performs a POST request with retry logic
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="request">Request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest request, 
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            _logger.LogDebug("Making POST request to: {Endpoint}", endpoint);
            
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            return await ProcessResponseAsync<TResponse>(response);
        });
    }

    /// <summary>
    /// Downloads a file from the specified URL
    /// </summary>
    /// <param name="url">Download URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File content as byte array</returns>
    public async Task<ApiResponse<byte[]>> DownloadFileAsync(string url, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            _logger.LogDebug("Downloading file from: {Url}", url);
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                return ApiResponse<byte[]>.Success(content, (int)response.StatusCode);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return ApiResponse<byte[]>.Error(
                    $"Download failed: {response.StatusCode} - {errorContent}",
                    (int)response.StatusCode);
            }
        });
    }

    /// <summary>
    /// Executes a request with retry logic
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <returns>API response</returns>
    private async Task<ApiResponse<T>> ExecuteWithRetryAsync<T>(Func<Task<ApiResponse<T>>> operation)
    {
        var attempt = 0;
        
        while (attempt <= _options.MaxRetryAttempts)
        {
            try
            {
                var result = await operation();
                
                // Don't retry on success or client errors (4xx)
                if (result.IsSuccess || (result.StatusCode >= 400 && result.StatusCode < 500))
                {
                    return result;
                }
                
                // Retry on server errors (5xx) or network issues
                if (attempt < _options.MaxRetryAttempts)
                {
                    var delay = TimeSpan.FromSeconds(_options.RetryDelaySeconds * Math.Pow(2, attempt));
                    _logger.LogWarning("Request failed (attempt {Attempt}/{MaxAttempts}), retrying in {Delay}ms", 
                        attempt + 1, _options.MaxRetryAttempts + 1, delay.TotalMilliseconds);
                    
                    await Task.Delay(delay);
                }
                
                attempt++;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception on attempt {Attempt}", attempt + 1);
                
                if (attempt >= _options.MaxRetryAttempts)
                {
                    return ApiResponse<T>.Error($"HTTP request failed after {_options.MaxRetryAttempts + 1} attempts: {ex.Message}");
                }
                
                var delay = TimeSpan.FromSeconds(_options.RetryDelaySeconds * Math.Pow(2, attempt));
                await Task.Delay(delay);
                attempt++;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Request timeout on attempt {Attempt}", attempt + 1);
                
                if (attempt >= _options.MaxRetryAttempts)
                {
                    return ApiResponse<T>.Error($"Request timed out after {_options.MaxRetryAttempts + 1} attempts");
                }
                
                var delay = TimeSpan.FromSeconds(_options.RetryDelaySeconds * Math.Pow(2, attempt));
                await Task.Delay(delay);
                attempt++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error on attempt {Attempt}", attempt + 1);
                return ApiResponse<T>.Error($"Unexpected error: {ex.Message}");
            }
        }
        
        return ApiResponse<T>.Error($"Request failed after {_options.MaxRetryAttempts + 1} attempts");
    }

    /// <summary>
    /// Processes HTTP response and deserializes content
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="response">HTTP response</param>
    /// <returns>API response</returns>
    private async Task<ApiResponse<T>> ProcessResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var statusCode = (int)response.StatusCode;
        
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                return ApiResponse<T>.Success(data!, statusCode);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response: {Content}", content);
                return ApiResponse<T>.Error($"Failed to deserialize response: {ex.Message}", statusCode);
            }
        }
        else
        {
            _logger.LogWarning("Request failed with status {StatusCode}: {Content}", statusCode, content);
            
            // Try to parse error response
            try
            {
                var errorResponse = JsonSerializer.Deserialize<RpcStatus>(content, _jsonOptions);
                return ApiResponse<T>.Error(errorResponse?.Message ?? content, statusCode);
            }
            catch
            {
                return ApiResponse<T>.Error(content, statusCode);
            }
        }
    }

    /// <summary>
    /// Disposes the HTTP client
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
