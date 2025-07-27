using NCBI.Datasets.API.Models.Common;
using Xunit;

namespace NCBI.Datasets.API.Tests.Models;

public class ApiResponseTests
{
    [Fact]
    public void Success_CreatesSuccessfulResponse()
    {
        // Arrange
        var testData = "test data";
        var statusCode = 200;

        // Act
        var response = ApiResponse<string>.Success(testData, statusCode);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(testData, response.Data);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public void Success_WithDefaultStatusCode_Uses200()
    {
        // Arrange
        var testData = "test data";

        // Act
        var response = ApiResponse<string>.Success(testData);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(testData, response.Data);
        Assert.Equal(200, response.StatusCode);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public void Error_CreatesErrorResponse()
    {
        // Arrange
        var errorMessage = "Something went wrong";
        var statusCode = 500;

        // Act
        var response = ApiResponse<string>.Error(errorMessage, statusCode);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal(errorMessage, response.ErrorMessage);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Null(response.Data);
    }

    [Fact]
    public void Error_WithDefaultStatusCode_Uses500()
    {
        // Arrange
        var errorMessage = "Something went wrong";

        // Act
        var response = ApiResponse<string>.Error(errorMessage);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal(errorMessage, response.ErrorMessage);
        Assert.Equal(500, response.StatusCode);
        Assert.Null(response.Data);
    }

    [Fact]
    public void ApiError_PropertiesSetCorrectly()
    {
        // Arrange & Act
        var apiError = new ApiError
        {
            Code = 404,
            Message = "Not found",
            Details = new List<object> { "Additional detail" }
        };

        // Assert
        Assert.Equal(404, apiError.Code);
        Assert.Equal("Not found", apiError.Message);
        Assert.NotNull(apiError.Details);
        Assert.Single(apiError.Details);
    }

    [Fact]
    public void RpcStatus_PropertiesSetCorrectly()
    {
        // Arrange & Act
        var rpcStatus = new RpcStatus
        {
            Code = 3,
            Message = "Invalid argument",
            Details = new List<object> { "Field validation failed" }
        };

        // Assert
        Assert.Equal(3, rpcStatus.Code);
        Assert.Equal("Invalid argument", rpcStatus.Message);
        Assert.NotNull(rpcStatus.Details);
        Assert.Single(rpcStatus.Details);
    }

    [Fact]
    public void ApiError_DefaultConstructor_InitializesEmptyMessage()
    {
        // Arrange & Act
        var apiError = new ApiError();

        // Assert
        Assert.Equal(0, apiError.Code);
        Assert.Equal(string.Empty, apiError.Message);
        Assert.Null(apiError.Details);
    }

    [Fact]
    public void RpcStatus_DefaultConstructor_InitializesEmptyMessage()
    {
        // Arrange & Act
        var rpcStatus = new RpcStatus();

        // Assert
        Assert.Equal(0, rpcStatus.Code);
        Assert.Equal(string.Empty, rpcStatus.Message);
        Assert.Null(rpcStatus.Details);
    }
}
