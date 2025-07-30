using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCBI.Datasets.API.Extensions;
using NCBI.Datasets.API.Models.Enums;
using Xunit;

namespace NCBI.Datasets.API.Tests.Integration;

/// <summary>
/// Integration tests for NCBI Datasets API client
/// These tests require a valid API key and internet connection
/// </summary>
public class NCBIDatasetsClientIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly INCBIDatasetsClient _client;

    public NCBIDatasetsClientIntegrationTests()
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddNCBIDatasetsApi(configuration);

        _serviceProvider = services.BuildServiceProvider();
        _client = _serviceProvider.GetRequiredService<INCBIDatasetsClient>();
    }

    [Fact]
    public async Task GetGenomeDownloadSummaryAsync_WithValidHumanAccession_ReturnsSuccess()
    {
        // Arrange
        var humanAccession = "GCF_000001405.40"; // Human reference genome

        // Act
        var response = await _client.GetGenomeDownloadSummaryAsync(humanAccession);

        // Assert
        Assert.True(response.IsSuccess, $"Request failed: {response.ErrorMessage}");
        Assert.NotNull(response.Data);
        Assert.True(response.Data.RecordCount > 0);
        Assert.NotNull(response.Data.Hydrated);
        Assert.NotNull(response.Data.Dehydrated);
        Assert.False(string.IsNullOrEmpty(response.Data.Hydrated.Url));
        Assert.False(string.IsNullOrEmpty(response.Data.Dehydrated.Url));
    }

    [Fact]
    public async Task GetGenomeDownloadSummaryAsync_WithMultipleAccessions_ReturnsSuccess()
    {
        // Arrange
        var accessions = new[] 
        { 
            "GCF_000001405.40", // Human
            "GCF_000001635.27"  // Mouse
        };

        // Act
        var response = await _client.GetGenomeDownloadSummaryAsync(accessions);

        // Assert
        Assert.True(response.IsSuccess, $"Request failed: {response.ErrorMessage}");
        Assert.NotNull(response.Data);
        Assert.True(response.Data.RecordCount >= 2);
        Assert.NotNull(response.Data.Hydrated);
        Assert.NotNull(response.Data.Dehydrated);
    }

    [Fact]
    public async Task GetGenomeDownloadSummaryAsync_WithChromosomesAndAnnotations_ReturnsSuccess()
    {
        // Arrange
        var accession = "GCF_000001405.40"; // Human reference genome
        var chromosomes = new[] { "1", "2", "X", "Y" };
        var annotationTypes = new[] 
        { 
            AnnotationForAssemblyType.GENOME_FASTA,
            AnnotationForAssemblyType.PROT_FASTA,
            AnnotationForAssemblyType.RNA_FASTA
        };

        // Act
        var response = await _client.GetGenomeDownloadSummaryAsync(
            accession, 
            chromosomes, 
            annotationTypes);

        // Assert
        Assert.True(response.IsSuccess, $"Request failed: {response.ErrorMessage}");
        Assert.NotNull(response.Data);
        Assert.True(response.Data.RecordCount > 0);
        Assert.NotNull(response.Data.AvailableFiles);
    }

    [Fact]
    public async Task GetGenomeDownloadSummaryAsync_WithInvalidAccession_ReturnsError()
    {
        // Arrange
        var invalidAccession = "INVALID_ACCESSION_123";

        // Act
        var response = await _client.GetGenomeDownloadSummaryAsync(invalidAccession);

        // Assert
        // The API might return success with 0 records or an error - both are acceptable
        if (response.IsSuccess)
        {
            Assert.Equal(0, response.Data?.RecordCount ?? 0);
        }
        else
        {
            Assert.False(response.IsSuccess);
            Assert.NotNull(response.ErrorMessage);
        }
    }

    [Fact]
    public async Task GetGenomeDownloadSummaryAsync_WithEmptyAccession_ReturnsError()
    {
        // Arrange
        var emptyAccession = "";

        // Act
        var response = await _client.GetGenomeDownloadSummaryAsync(emptyAccession);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal(400, response.StatusCode);
        Assert.Contains("cannot be null or empty", response.ErrorMessage);
    }

    [Fact]
    public async Task GetGenomeDownloadSummaryByPostAsync_WithValidAccessions_ReturnsSuccess()
    {
        // Arrange
        var request = new NCBI.Datasets.API.Models.Genome.GenomeDownloadSummaryRequest
        {
            Accessions = new List<string> { "GCF_000001405.40", "GCF_000001635.27" },
            Chromosomes = new List<string> { "1", "2" },
            IncludeAnnotationTypes = new List<AnnotationForAssemblyType> 
            { 
                AnnotationForAssemblyType.GENOME_FASTA 
            }
        };

        // Act
        var response = await _client.GetGenomeDownloadSummaryByPostAsync(request);

        // Assert
        Assert.True(response.IsSuccess, $"Request failed: {response.ErrorMessage}");
        Assert.NotNull(response.Data);
        Assert.True(response.Data.RecordCount >= 2);
    }

    [Fact]
    public async Task DownloadGenomeDatasetAsync_WithValidUrl_ReturnsData()
    {
        // Arrange - First get a download summary to get a valid URL
        var accession = "GCF_000005825.2"; // Small E. coli genome for faster download
        var summaryResponse = await _client.GetGenomeDownloadSummaryAsync(accession);
        
        Assert.True(summaryResponse.IsSuccess, "Failed to get download summary");
        Assert.NotNull(summaryResponse.Data?.Hydrated?.Url);

        var downloadUrl = summaryResponse.Data.Hydrated.Url;

        // Act
        var downloadResponse = await _client.DownloadGenomeDatasetAsync(downloadUrl);

        // Assert
        Assert.True(downloadResponse.IsSuccess, $"Download failed: {downloadResponse.ErrorMessage}");
        Assert.NotNull(downloadResponse.Data);
        Assert.True(downloadResponse.Data.Length > 0);
    }

    [Fact]
    public async Task DownloadGenomeDatasetAsync_WithInvalidUrl_ReturnsError()
    {
        // Arrange
        var invalidUrl = "https://invalid-url-that-does-not-exist.com/file.zip";

        // Act
        var response = await _client.DownloadGenomeDatasetAsync(invalidUrl);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task DownloadGenomeDatasetAsync_WithEmptyUrl_ReturnsError()
    {
        // Arrange
        var emptyUrl = "";

        // Act
        var response = await _client.DownloadGenomeDatasetAsync(emptyUrl);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal(400, response.StatusCode);
        Assert.Contains("cannot be null or empty", response.ErrorMessage);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _serviceProvider?.Dispose();
    }
}
