using NCBI.Datasets.API.Configuration;
using Xunit;

namespace NCBI.Datasets.API.Tests.Configuration;

public class NCBIDatasetsApiOptionsTests
{
    [Fact]
    public void IsValid_WithValidOptions_ReturnsTrue()
    {
        // Arrange
        var options = new NCBIDatasetsApiOptions
        {
            BaseUrl = "https://api.ncbi.nlm.nih.gov/datasets/v2",
            ApiKey = "test-api-key",
            TimeoutSeconds = 30,
            MaxRetryAttempts = 3,
            RetryDelaySeconds = 1
        };

        // Act
        var result = options.IsValid();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("", "test-api-key", 30, 3, 1)] // Empty BaseUrl
    [InlineData("https://api.ncbi.nlm.nih.gov/datasets/v2", "", 30, 3, 1)] // Empty ApiKey
    [InlineData("https://api.ncbi.nlm.nih.gov/datasets/v2", "test-api-key", 0, 3, 1)] // Zero timeout
    [InlineData("https://api.ncbi.nlm.nih.gov/datasets/v2", "test-api-key", -1, 3, 1)] // Negative timeout
    [InlineData("https://api.ncbi.nlm.nih.gov/datasets/v2", "test-api-key", 30, -1, 1)] // Negative retry attempts
    [InlineData("https://api.ncbi.nlm.nih.gov/datasets/v2", "test-api-key", 30, 3, -1)] // Negative retry delay
    public void IsValid_WithInvalidOptions_ReturnsFalse(
        string baseUrl, 
        string apiKey, 
        int timeoutSeconds, 
        int maxRetryAttempts, 
        int retryDelaySeconds)
    {
        // Arrange
        var options = new NCBIDatasetsApiOptions
        {
            BaseUrl = baseUrl,
            ApiKey = apiKey,
            TimeoutSeconds = timeoutSeconds,
            MaxRetryAttempts = maxRetryAttempts,
            RetryDelaySeconds = retryDelaySeconds
        };

        // Act
        var result = options.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void SectionName_HasCorrectValue()
    {
        // Assert
        Assert.Equal("NCBIDatasetsApi", NCBIDatasetsApiOptions.SectionName);
    }

    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var options = new NCBIDatasetsApiOptions();

        // Assert
        Assert.Equal("https://api.ncbi.nlm.nih.gov/datasets/v2", options.BaseUrl);
        Assert.Equal(string.Empty, options.ApiKey);
        Assert.Equal(30, options.TimeoutSeconds);
        Assert.Equal(3, options.MaxRetryAttempts);
        Assert.Equal(1, options.RetryDelaySeconds);
    }
}
