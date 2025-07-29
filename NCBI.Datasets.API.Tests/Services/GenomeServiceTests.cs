using Microsoft.Extensions.Logging;
using Moq;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Download;
using NCBI.Datasets.API.Models.Genome;
using NCBI.Datasets.API.Models.Enums;
using NCBI.Datasets.API.Services;
using System.Text.Json;
using Xunit;

namespace NCBI.Datasets.API.Tests.Services;

public class GenomeServiceTests
{
    private readonly Mock<INCBIDatasetsHttpClient> _mockHttpClient;
    private readonly Mock<ILogger<GenomeService>> _mockLogger;
    private readonly GenomeService _genomeService;

    public GenomeServiceTests()
    {
        _mockHttpClient = new Mock<INCBIDatasetsHttpClient>();
        _mockLogger = new Mock<ILogger<GenomeService>>();
        _genomeService = new GenomeService(_mockHttpClient.Object, _mockLogger.Object);
    }

    #region Assembly Dataset Availability Tests

    [Fact]
    public async Task CheckAssemblyDatasetAvailabilityAsync_WithAccessions_ReturnsSuccess()
    {
        // Arrange
        var accessions = new[] { "GCF_000001405.40", "GCF_000002305.1" };
        var expectedResponse = new AssemblyDatasetAvailability
        {
            ValidAssemblies = new List<string> { "GCF_000001405.40" },
            InvalidAssemblies = new List<string> { "GCF_000002305.1" }
        };
        var apiResponse = ApiResponse<AssemblyDatasetAvailability>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<AssemblyDatasetAvailability>("genome/dataset/GCF_000001405.40,GCF_000002305.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.CheckAssemblyDatasetAvailabilityAsync(accessions);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.ValidAssemblies);
        Assert.Single(result.Data.InvalidAssemblies);
        Assert.Equal("GCF_000001405.40", result.Data.ValidAssemblies[0]);
    }

    [Fact]
    public async Task CheckAssemblyDatasetAvailabilityAsync_WithNullAccessions_ReturnsError()
    {
        // Act
        var result = await _genomeService.CheckAssemblyDatasetAvailabilityAsync((IEnumerable<string>)null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    [Fact]
    public async Task CheckAssemblyDatasetAvailabilityAsync_WithRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new AssemblyDatasetRequest
        {
            Accessions = new List<string> { "GCF_000001405.40" },
            Hydrated = AssemblyDatasetRequestResolution.Chromosome
        };
        var expectedResponse = new AssemblyDatasetAvailability
        {
            ValidAssemblies = new List<string> { "GCF_000001405.40" },
            InvalidAssemblies = new List<string>()
        };
        var apiResponse = ApiResponse<AssemblyDatasetAvailability>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<AssemblyDatasetRequest, AssemblyDatasetAvailability>("genome/dataset", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.CheckAssemblyDatasetAvailabilityAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.ValidAssemblies);
        Assert.Empty(result.Data.InvalidAssemblies);
    }

    #endregion

    #region Assembly Dataset Reports Tests

    [Fact]
    public async Task GetAssemblyDatasetReportsAsync_WithAccessions_ReturnsSuccess()
    {
        // Arrange
        var accessions = new[] { "GCF_000001405.40" };
        var expectedResponse = new AssemblyDatasetReport
        {
            TotalCount = 1,
            Reports = new List<AssemblyReport>
            {
                new AssemblyReport
                {
                    Accession = "GCF_000001405.40",
                    Organism = new OrganismInfo
                    {
                        OrganismName = "Homo sapiens",
                        TaxId = 9606
                    },
                    AssemblyInfo = new AssemblyInfo
                    {
                        AssemblyName = "GRCh38.p14"
                    }
                }
            }
        };
        var apiResponse = ApiResponse<AssemblyDatasetReport>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<AssemblyDatasetReport>("genome/dataset_report/GCF_000001405.40", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.GetAssemblyDatasetReportsAsync(accessions);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TotalCount);
        Assert.Single(result.Data.Reports);
        Assert.Equal("GCF_000001405.40", result.Data.Reports[0].Accession);
    }

    [Fact]
    public async Task GetAssemblyDatasetReportsAsync_WithPagination_BuildsCorrectUrl()
    {
        // Arrange
        var accessions = new[] { "GCF_000001405.40" };
        var pageSize = 10;
        var pageToken = "next_page_token";
        var returnedContent = AssemblyDatasetReportsRequestContentType.Complete;
        
        var expectedResponse = new AssemblyDatasetReport { TotalCount = 1, Reports = new List<AssemblyReport>() };
        var apiResponse = ApiResponse<AssemblyDatasetReport>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<AssemblyDatasetReport>(
                "genome/dataset_report/GCF_000001405.40?page_size=10&page_token=next_page_token&returned_content=Complete", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.GetAssemblyDatasetReportsAsync(accessions, pageSize, pageToken, returnedContent);

        // Assert
        Assert.True(result.IsSuccess);
        _mockHttpClient.Verify(x => x.GetAsync<AssemblyDatasetReport>(
            "genome/dataset_report/GCF_000001405.40?page_size=10&page_token=next_page_token&returned_content=Complete", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAssemblyDatasetReportsAsync_WithRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new AssemblyDatasetRequest
        {
            Accessions = new List<string> { "GCF_000001405.40" }
        };
        var expectedResponse = new AssemblyDatasetReport { TotalCount = 1, Reports = new List<AssemblyReport>() };
        var apiResponse = ApiResponse<AssemblyDatasetReport>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<AssemblyDatasetRequest, AssemblyDatasetReport>("genome/dataset_report", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.GetAssemblyDatasetReportsAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    #endregion

    #region Download Operations Tests

    [Fact]
    public async Task DownloadGenomeDatasetAsync_WithAccessions_ReturnsSuccess()
    {
        // Arrange
        var accessions = new[] { "GCF_000001405.40" };
        var annotationTypes = new[] { AnnotationTypes.Genome, AnnotationTypes.Rna };
        var hydrated = HydrationLevel.Fully_Hydrated;
        var filename = "genome_data.zip";
        
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        var apiResponse = ApiResponse<byte[]>.Success(expectedData);

        _mockHttpClient
            .Setup(x => x.DownloadFileAsync(
                "genome/accession/GCF_000001405.40/download?include_annotation_type=Genome&include_annotation_type=Rna&hydrated=Fully_Hydrated&filename=genome_data.zip", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.DownloadGenomeDatasetAsync(accessions, annotationTypes, hydrated, filename);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedData, result.Data);
    }

    [Fact]
    public async Task DownloadGenomeDatasetAsync_WithRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new GenomeDownloadRequest
        {
            Accessions = new List<string> { "GCF_000001405.40" },
            IncludeAnnotationTypes = new List<AnnotationForAssemblyType> { AnnotationForAssemblyType.GENOME_FASTA }
        };
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        var apiResponse = ApiResponse<byte[]>.Success(expectedData);

        _mockHttpClient
            .Setup(x => x.PostDownloadAsync("genome/accession/download", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.DownloadGenomeDatasetAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedData, result.Data);
    }

    #endregion

    #region Download Summary Tests

    [Fact]
    public async Task GetDownloadSummaryAsync_WithSingleAccession_UsesGetEndpoint()
    {
        // Arrange
        var request = new GenomeDownloadSummaryRequest
        {
            Accessions = new List<string> { "GCF_000001405.40" }
        };
        var expectedResponse = new DownloadSummary
        {
            RecordCount = 1,
            Hydrated = new DownloadSummaryHydrated
            {
                EstimatedFileSizeMb = 1,
                Url = "https://example.com/download"
            }
        };
        var apiResponse = ApiResponse<DownloadSummary>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<DownloadSummary>("genome/accession/GCF_000001405.40/download_summary", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.GetDownloadSummaryAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.RecordCount);
        _mockHttpClient.Verify(x => x.GetAsync<DownloadSummary>(
            "genome/accession/GCF_000001405.40/download_summary", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDownloadSummaryAsync_WithMultipleAccessions_UsesPostEndpoint()
    {
        // Arrange
        var request = new GenomeDownloadSummaryRequest
        {
            Accessions = new List<string> { "GCF_000001405.40", "GCF_000002305.1" }
        };
        var expectedResponse = new DownloadSummary { RecordCount = 2 };
        var apiResponse = ApiResponse<DownloadSummary>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<GenomeDownloadSummaryPostRequest, DownloadSummary>(
                "genome/download_summary", 
                It.IsAny<GenomeDownloadSummaryPostRequest>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.GetDownloadSummaryAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        _mockHttpClient.Verify(x => x.PostAsync<GenomeDownloadSummaryPostRequest, DownloadSummary>(
            "genome/download_summary", 
            It.IsAny<GenomeDownloadSummaryPostRequest>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDownloadSummaryAsync_WithInvalidRequest_ReturnsError()
    {
        // Arrange
        var request = new GenomeDownloadSummaryRequest
        {
            Accessions = new List<string>() // Empty accessions
        };

        // Act
        var result = await _genomeService.GetDownloadSummaryAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("invalid", result.ErrorMessage);
    }

    #endregion

    #region Assembly Links Tests

    [Fact]
    public async Task GetAssemblyLinksAsync_WithAccessions_ReturnsSuccess()
    {
        // Arrange
        var accessions = new[] { "GCF_000001405.40" };
        var linkType = AssemblyLinkType.Stats;
        var expectedResponse = new AssemblyLinksReply
        {
            AssemblyLinks = new List<AssemblyLink>
            {
                new AssemblyLink
                {
                    Accession = "GCF_000001405.40",
                    Url = "https://example.com/stats",
                    LinkType = "Stats"
                }
            }
        };
        var apiResponse = ApiResponse<AssemblyLinksReply>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<AssemblyLinksReply>("genome/accession/GCF_000001405.40/links?link_type=Stats", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.GetAssemblyLinksAsync(accessions, linkType);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.AssemblyLinks);
        Assert.Equal("GCF_000001405.40", result.Data.AssemblyLinks[0].Accession);
    }

    #endregion

    #region Sequence Operations Tests

    [Fact]
    public async Task GetSequenceAssembliesAsync_WithAccessions_ReturnsSuccess()
    {
        // Arrange
        var sequenceAccessions = new[] { "NC_000001.11" };
        var expectedResponse = new SequenceAssembliesReply
        {
            SequenceAssemblies = new List<SequenceAssembly>
            {
                new SequenceAssembly
                {
                    SequenceAccession = "NC_000001.11",
                    AssemblyAccession = "GCF_000001405.40",
                    SequenceLength = 248956422,
                    SequenceRole = "assembled-molecule"
                }
            }
        };
        var apiResponse = ApiResponse<SequenceAssembliesReply>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<SequenceAssembliesReply>("genome/sequence/NC_000001.11/assemblies", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.GetSequenceAssembliesAsync(sequenceAccessions);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.SequenceAssemblies);
        Assert.Equal("NC_000001.11", result.Data.SequenceAssemblies[0].SequenceAccession);
    }

    [Fact]
    public async Task GetSequenceReportsAsync_WithAccessions_ReturnsSuccess()
    {
        // Arrange
        var sequenceAccessions = new[] { "NC_000001.11" };
        var expectedResponse = new SequenceReportsReply
        {
            SequenceReports = new List<SequenceReport>
            {
                new SequenceReport
                {
                    Accession = "NC_000001.11",
                    Length = 248956422,
                    SequenceName = "chromosome 1",
                    ChromosomeName = "1",
                    AssemblyUnit = "Primary Assembly"
                }
            }
        };
        var apiResponse = ApiResponse<SequenceReportsReply>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<SequenceReportsReply>("genome/sequence/NC_000001.11/reports", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.GetSequenceReportsAsync(sequenceAccessions);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.SequenceReports);
        Assert.Equal("NC_000001.11", result.Data.SequenceReports[0].Accession);
    }

    #endregion

    #region CheckM Operations Tests

    [Fact]
    public async Task GetCheckMHistogramAsync_WithSpeciesTaxon_ReturnsSuccess()
    {
        // Arrange
        var speciesTaxon = "9606";
        var expectedResponse = new AssemblyCheckMHistogramReply
        {
            SpeciesTaxid = 9606,
            HistogramIntervals = new List<AssemblyCheckMHistogramInterval>
            {
                new AssemblyCheckMHistogramInterval
                {
                    StartPos = 0.0f,
                    StopPos = 10.0f,
                    Count = 5.0f
                }
            }
        };
        var apiResponse = ApiResponse<AssemblyCheckMHistogramReply>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<AssemblyCheckMHistogramReply>("genome/taxon/9606/checkm_histogram", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.GetCheckMHistogramAsync(speciesTaxon);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(9606, result.Data.SpeciesTaxid);
        Assert.Single(result.Data.HistogramIntervals);
    }

    [Fact]
    public async Task GetCheckMHistogramAsync_WithNullSpeciesTaxon_ReturnsError()
    {
        // Act
        var result = await _genomeService.GetCheckMHistogramAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Check Operations Tests

    [Fact]
    public async Task CheckGenomeDatasetAsync_WithAccessions_ReturnsSuccess()
    {
        // Arrange
        var accessions = new[] { "GCF_000001405.40" };
        var expectedResponse = new CheckResult
        {
            IsValid = true,
            Message = "Dataset is valid"
        };
        var apiResponse = ApiResponse<CheckResult>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<CheckResult>("genome/accession/GCF_000001405.40/check", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.CheckGenomeDatasetAsync(accessions);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsValid);
    }

    [Fact]
    public async Task CheckGenomeDatasetAsync_WithRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new GenomeCheckRequest
        {
            Accessions = new List<string> { "GCF_000001405.40" }
        };
        var expectedResponse = new CheckResult { IsValid = true };
        var apiResponse = ApiResponse<CheckResult>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<GenomeCheckRequest, CheckResult>("genome/accession/check", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.CheckGenomeDatasetAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsValid);
    }

    #endregion

    #region Legacy Download Operations Tests

    [Fact]
    public async Task DownloadDatasetAsync_WithValidUrl_ReturnsSuccess()
    {
        // Arrange
        var downloadUrl = "https://api.ncbi.nlm.nih.gov/datasets/v2/download/genome/accession/GCF_000001405.40";
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        var apiResponse = ApiResponse<byte[]>.Success(expectedData);

        _mockHttpClient
            .Setup(x => x.DownloadFileAsync(downloadUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _genomeService.DownloadDatasetAsync(downloadUrl);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedData, result.Data);
    }

    [Fact]
    public async Task DownloadDatasetAsync_WithNullUrl_ReturnsError()
    {
        // Act
        var result = await _genomeService.DownloadDatasetAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task CheckAssemblyDatasetAvailabilityAsync_WhenHttpClientThrows_ReturnsError()
    {
        // Arrange
        var accessions = new[] { "GCF_000001405.40" };
        _mockHttpClient
            .Setup(x => x.GetAsync<AssemblyDatasetAvailability>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _genomeService.CheckAssemblyDatasetAvailabilityAsync(accessions);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unexpected error", result.ErrorMessage);
    }

    #endregion
}
