using System.Text.Json.Serialization;

namespace NCBI.Datasets.API.Models.Common;

/// <summary>
/// Base class for API responses
/// </summary>
/// <typeparam name="T">The type of data returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// The response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error message if the request failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Creates a successful response
    /// </summary>
    /// <param name="data">The response data</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <returns>Successful API response</returns>
    public static ApiResponse<T> Success(T data, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Data = data,
            IsSuccess = true,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <returns>Error API response</returns>
    public static ApiResponse<T> Error(string errorMessage, int statusCode = 500)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
    }
}

/// <summary>
/// Error information from API responses
/// </summary>
public class ApiError
{
    /// <summary>
    /// Error code
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details
    /// </summary>
    [JsonPropertyName("details")]
    public List<object>? Details { get; set; }
}

/// <summary>
/// RPC status response for error handling
/// </summary>
public class RpcStatus
{
    /// <summary>
    /// Status code
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Status message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional details
    /// </summary>
    [JsonPropertyName("details")]
    public List<object>? Details { get; set; }
}
