using Microsoft.Extensions.Logging;
using Moq;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Download;
using NCBI.Datasets.API.Models.Gene;
using NCBI.Datasets.API.Models.Enums;
using NCBI.Datasets.API.Services;
using Xunit;

namespace NCBI.Datasets.API.Tests.Services;

public class GeneServiceTests
{
    private readonly Mock<INCBIDatasetsHttpClient> _mockHttpClient;
    private readonly Mock<ILogger<GeneService>> _mockLogger;
    private readonly GeneService _geneService;

    public GeneServiceTests()
    {
        _mockHttpClient = new Mock<INCBIDatasetsHttpClient>();
        _mockLogger = new Mock<ILogger<GeneService>>();
        _geneService = new GeneService(_mockHttpClient.Object, _mockLogger.Object);
    }

    #region Gene Reports by ID Tests

    [Fact]
    public async Task GetGeneReportsByIdAsync_WithValidGeneIds_ReturnsSuccess()
    {
        // Arrange
        var geneIds = new[] { 59067, 50615 };
        var expectedResponse = new GeneDataReportPage
        {
            Reports = new List<GeneReportMatch>
            {
                new GeneReportMatch
                {
                    Gene = new GeneDescriptor
                    {
                        GeneId = 59067,
                        Symbol = "IL21",
                        Description = "interleukin 21",
                        GeneType = "protein-coding"
                    }
                }
            },
            TotalCount = 1
        };
        var apiResponse = ApiResponse<GeneDataReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<GeneDataReportPage>("/gene/id/59067,50615", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneReportsByIdAsync(geneIds);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Reports);
        Assert.Equal(59067, result.Data.Reports[0].Gene!.GeneId);
        Assert.Equal("IL21", result.Data.Reports[0].Gene!.Symbol);
    }

    [Fact]
    public async Task GetGeneReportsByIdAsync_WithPagination_BuildsCorrectUrl()
    {
        // Arrange
        var geneIds = new[] { 59067 };
        var pageSize = 10;
        var pageToken = "next_page_token";
        var returnedContent = GeneDatasetReportsRequestContentType.COMPLETE;
        
        var expectedResponse = new GeneDataReportPage { TotalCount = 1, Reports = new List<GeneReportMatch>() };
        var apiResponse = ApiResponse<GeneDataReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<GeneDataReportPage>(
                "/gene/id/59067?returned_content=COMPLETE&page_size=10&page_token=next_page_token", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneReportsByIdAsync(geneIds, returnedContent, pageSize, pageToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockHttpClient.Verify(x => x.GetAsync<GeneDataReportPage>(
            "/gene/id/59067?returned_content=COMPLETE&page_size=10&page_token=next_page_token", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGeneReportsByIdAsync_WithNullGeneIds_ReturnsError()
    {
        // Act
        var result = await _geneService.GetGeneReportsByIdAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    [Fact]
    public async Task GetGeneReportsByIdAsync_WithEmptyGeneIds_ReturnsError()
    {
        // Act
        var result = await _geneService.GetGeneReportsByIdAsync(new int[0]);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Gene Reports by Accession Tests

    [Fact]
    public async Task GetGeneReportsByAccessionAsync_WithValidAccessions_ReturnsSuccess()
    {
        // Arrange
        var accessions = new[] { "NM_021803.4", "NP_068575.1" };
        var expectedResponse = new GeneDataReportPage
        {
            Reports = new List<GeneReportMatch>
            {
                new GeneReportMatch
                {
                    Gene = new GeneDescriptor
                    {
                        GeneId = 59067,
                        Symbol = "IL21",
                        Description = "interleukin 21"
                    }
                }
            },
            TotalCount = 1
        };
        var apiResponse = ApiResponse<GeneDataReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<GeneDataReportPage>("/gene/accession/NM_021803.4,NP_068575.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneReportsByAccessionAsync(accessions);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Reports);
        Assert.Equal("IL21", result.Data.Reports[0].Gene!.Symbol);
    }

    [Fact]
    public async Task GetGeneReportsByAccessionAsync_WithNullAccessions_ReturnsError()
    {
        // Act
        var result = await _geneService.GetGeneReportsByAccessionAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Gene Reports by Symbol Tests

    [Fact]
    public async Task GetGeneReportsBySymbolAsync_WithValidSymbolAndTaxon_ReturnsSuccess()
    {
        // Arrange
        var symbols = new[] { "GNAS" };
        var taxon = "9606";
        var expectedResponse = new GeneDataReportPage
        {
            Reports = new List<GeneReportMatch>
            {
                new GeneReportMatch
                {
                    Gene = new GeneDescriptor
                    {
                        GeneId = 2778,
                        Symbol = "GNAS",
                        Description = "GNAS complex locus"
                    }
                }
            },
            TotalCount = 1
        };
        var apiResponse = ApiResponse<GeneDataReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<GeneDataReportPage>("/gene/symbol/GNAS/taxon/9606", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneReportsBySymbolAsync(symbols, taxon);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Reports);
        Assert.Equal("GNAS", result.Data.Reports[0].Gene!.Symbol);
    }

    [Fact]
    public async Task GetGeneReportsBySymbolAsync_WithNullSymbols_ReturnsError()
    {
        // Act
        var result = await _geneService.GetGeneReportsBySymbolAsync(null!, "9606");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    [Fact]
    public async Task GetGeneReportsBySymbolAsync_WithNullTaxon_ReturnsError()
    {
        // Act
        var result = await _geneService.GetGeneReportsBySymbolAsync(new[] { "GNAS" }, null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Gene Reports by Taxon Tests

    [Fact]
    public async Task GetGeneReportsByTaxonAsync_WithValidTaxon_ReturnsSuccess()
    {
        // Arrange
        var taxon = "9606";
        var expectedResponse = new GeneDataReportPage
        {
            Reports = new List<GeneReportMatch>(),
            TotalCount = 100
        };
        var apiResponse = ApiResponse<GeneDataReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<GeneDataReportPage>("/gene/taxon/9606", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneReportsByTaxonAsync(taxon);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(100, result.Data.TotalCount);
    }

    [Fact]
    public async Task GetGeneReportsByTaxonAsync_WithNullTaxon_ReturnsError()
    {
        // Act
        var result = await _geneService.GetGeneReportsByTaxonAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Gene Reports POST Tests

    [Fact]
    public async Task GetGeneReportsAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new GeneDatasetReportsRequest
        {
            GeneIds = new List<int> { 59067, 50615 },
            ReturnedContent = GeneDatasetReportsRequestContentType.COMPLETE
        };
        var expectedResponse = new GeneDataReportPage { TotalCount = 2, Reports = new List<GeneReportMatch>() };
        var apiResponse = ApiResponse<GeneDataReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<GeneDatasetReportsRequest, GeneDataReportPage>("/gene", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneReportsAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.TotalCount);
    }

    [Fact]
    public async Task GetGeneReportsAsync_WithNullRequest_ReturnsError()
    {
        // Act
        var result = await _geneService.GetGeneReportsAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null", result.ErrorMessage);
    }

    #endregion

    #region Gene Download Tests

    [Fact]
    public async Task DownloadGeneDatasetByIdAsync_WithValidGeneIds_ReturnsSuccess()
    {
        // Arrange
        var geneIds = new[] { 59067 };
        var annotationTypes = new[] { GeneAnnotationType.GENE_FASTA, GeneAnnotationType.PROTEIN_FASTA };
        var filename = "gene_data.zip";
        
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        var apiResponse = ApiResponse<byte[]>.Success(expectedData);

        _mockHttpClient
            .Setup(x => x.DownloadFileAsync(
                "/gene/id/59067/download?include_annotation_type=GENE_FASTA&include_annotation_type=PROTEIN_FASTA&filename=gene_data.zip", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.DownloadGeneDatasetByIdAsync(geneIds, annotationTypes, filename);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedData, result.Data);
    }

    [Fact]
    public async Task DownloadGeneDatasetAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new GeneDatasetRequest
        {
            GeneIds = new List<int> { 59067 },
            IncludeAnnotationType = new List<GeneAnnotationType> { GeneAnnotationType.GENE_FASTA }
        };
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        var apiResponse = ApiResponse<byte[]>.Success(expectedData);

        _mockHttpClient
            .Setup(x => x.PostDownloadAsync("/gene/download", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.DownloadGeneDatasetAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedData, result.Data);
    }

    [Fact]
    public async Task DownloadGeneDatasetByIdAsync_WithNullGeneIds_ReturnsError()
    {
        // Act
        var result = await _geneService.DownloadGeneDatasetByIdAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Gene Download Summary Tests

    [Fact]
    public async Task GetGeneDownloadSummaryByIdAsync_WithValidGeneIds_ReturnsSuccess()
    {
        // Arrange
        var geneIds = new[] { 59067 };
        var expectedResponse = new DownloadSummary
        {
            RecordCount = 1,
            Hydrated = new DownloadSummaryHydrated
            {
                EstimatedFileSizeMb = 5,
                Url = "https://example.com/download"
            }
        };
        var apiResponse = ApiResponse<DownloadSummary>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<DownloadSummary>("/gene/id/59067/download_summary", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneDownloadSummaryByIdAsync(geneIds);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.RecordCount);
    }

    [Fact]
    public async Task GetGeneDownloadSummaryAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new GeneDatasetRequest
        {
            GeneIds = new List<int> { 59067 }
        };
        var expectedResponse = new DownloadSummary { RecordCount = 1 };
        var apiResponse = ApiResponse<DownloadSummary>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<GeneDatasetRequest, DownloadSummary>("/gene/download_summary", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneDownloadSummaryAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.RecordCount);
    }

    #endregion

    #region Gene Counts by Taxon Tests

    [Fact]
    public async Task GetGeneCountsByTaxonAsync_WithValidTaxon_ReturnsSuccess()
    {
        // Arrange
        var taxon = "9606";
        var expectedResponse = new GeneCountsByTaxonReply
        {
            Taxon = "9606",
            Counts = new List<GeneTypeAndCount>
            {
                new GeneTypeAndCount { GeneType = "protein-coding", Count = 20000 },
                new GeneTypeAndCount { GeneType = "ncRNA", Count = 5000 }
            },
            GeneCounts = new GeneCounts
            {
                Total = 25000,
                ProteinCoding = 20000,
                NonCoding = 5000
            }
        };
        var apiResponse = ApiResponse<GeneCountsByTaxonReply>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<GeneCountsByTaxonReply>("/gene/taxon/9606/counts", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneCountsByTaxonAsync(taxon);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("9606", result.Data.Taxon);
        Assert.Equal(2, result.Data.Counts.Count);
        Assert.Equal(25000, result.Data.GeneCounts!.Total);
    }

    [Fact]
    public async Task GetGeneCountsByTaxonAsync_WithRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new GeneCountsByTaxonRequest { Taxon = "9606" };
        var expectedResponse = new GeneCountsByTaxonReply { Taxon = "9606" };
        var apiResponse = ApiResponse<GeneCountsByTaxonReply>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<GeneCountsByTaxonRequest, GeneCountsByTaxonReply>("/gene/taxon/counts", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneCountsByTaxonAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("9606", result.Data.Taxon);
    }

    [Fact]
    public async Task GetGeneCountsByTaxonAsync_WithNullTaxon_ReturnsError()
    {
        // Act
        var result = await _geneService.GetGeneCountsByTaxonAsync((string)null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Gene Orthologs Tests

    [Fact]
    public async Task GetGeneOrthologsByIdAsync_WithValidGeneId_ReturnsSuccess()
    {
        // Arrange
        var geneId = 2778;
        var expectedResponse = new GeneDataReportPage
        {
            Reports = new List<GeneReportMatch>
            {
                new GeneReportMatch
                {
                    Gene = new GeneDescriptor
                    {
                        GeneId = 2778,
                        Symbol = "GNAS",
                        Description = "GNAS complex locus"
                    }
                }
            },
            TotalCount = 1
        };
        var apiResponse = ApiResponse<GeneDataReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<GeneDataReportPage>("/gene/id/2778/orthologs", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneOrthologsByIdAsync(geneId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Reports);
        Assert.Equal(2778, result.Data.Reports[0].Gene!.GeneId);
    }

    [Fact]
    public async Task GetGeneOrthologsAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new OrthologRequest
        {
            GeneId = 2778,
            TaxonFilter = new List<string> { "10090", "10116" }
        };
        var expectedResponse = new GeneDataReportPage { TotalCount = 2 };
        var apiResponse = ApiResponse<GeneDataReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<OrthologRequest, GeneDataReportPage>("/gene/orthologs", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneOrthologsAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.TotalCount);
    }

    #endregion

    #region Gene Links Tests

    [Fact]
    public async Task GetGeneLinksByIdAsync_WithValidGeneIds_ReturnsSuccess()
    {
        // Arrange
        var geneIds = new[] { 59067 };
        var expectedResponse = new GeneLinksReply
        {
            Links = new List<GeneLink>
            {
                new GeneLink
                {
                    GeneId = 59067,
                    LinkType = "NCBI_GENE",
                    Url = "https://www.ncbi.nlm.nih.gov/gene/59067",
                    Description = "NCBI Gene page"
                }
            }
        };
        var apiResponse = ApiResponse<GeneLinksReply>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<GeneLinksReply>("/gene/id/59067/links", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneLinksByIdAsync(geneIds);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Links);
        Assert.Equal(59067, result.Data.Links[0].GeneId);
        Assert.Equal("NCBI_GENE", result.Data.Links[0].LinkType);
    }

    [Fact]
    public async Task GetGeneLinksAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new GeneLinksRequest
        {
            GeneIds = new List<int> { 59067 },
            LinkTypes = new List<GeneLinkType> { GeneLinkType.NCBI_GENE }
        };
        var expectedResponse = new GeneLinksReply { Links = new List<GeneLink>() };
        var apiResponse = ApiResponse<GeneLinksReply>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<GeneLinksRequest, GeneLinksReply>("/gene/links", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneLinksAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetGeneLinksByIdAsync_WithNullGeneIds_ReturnsError()
    {
        // Act
        var result = await _geneService.GetGeneLinksByIdAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Gene Chromosome Summary Tests

    [Fact]
    public async Task GetGeneChromosomeSummaryAsync_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var taxon = "9606";
        var annotationName = "GRCh38.p14";
        var expectedResponse = new GeneChromosomeSummaryReply
        {
            Taxon = "9606",
            AnnotationName = "GRCh38.p14",
            Chromosomes = new List<ChromosomeSummary>
            {
                new ChromosomeSummary
                {
                    Chromosome = "1",
                    GeneCounts = new GeneCounts { Total = 2000, ProteinCoding = 1500 },
                    Length = 248956422
                }
            }
        };
        var apiResponse = ApiResponse<GeneChromosomeSummaryReply>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<GeneChromosomeSummaryReply>("/gene/taxon/9606/annotation/GRCh38.p14/chromosome_summary", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _geneService.GetGeneChromosomeSummaryAsync(taxon, annotationName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("9606", result.Data.Taxon);
        Assert.Equal("GRCh38.p14", result.Data.AnnotationName);
        Assert.Single(result.Data.Chromosomes);
        Assert.Equal("1", result.Data.Chromosomes[0].Chromosome);
    }

    [Fact]
    public async Task GetGeneChromosomeSummaryAsync_WithNullTaxon_ReturnsError()
    {
        // Act
        var result = await _geneService.GetGeneChromosomeSummaryAsync(null!, "GRCh38.p14");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    [Fact]
    public async Task GetGeneChromosomeSummaryAsync_WithNullAnnotationName_ReturnsError()
    {
        // Act
        var result = await _geneService.GetGeneChromosomeSummaryAsync("9606", null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task GetGeneReportsByIdAsync_WhenHttpClientThrows_ReturnsError()
    {
        // Arrange
        var geneIds = new[] { 59067 };
        _mockHttpClient
            .Setup(x => x.GetAsync<GeneDataReportPage>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _geneService.GetGeneReportsByIdAsync(geneIds);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unexpected error", result.ErrorMessage);
    }

    [Fact]
    public async Task DownloadGeneDatasetByIdAsync_WhenHttpClientThrows_ReturnsError()
    {
        // Arrange
        var geneIds = new[] { 59067 };
        _mockHttpClient
            .Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _geneService.DownloadGeneDatasetByIdAsync(geneIds);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unexpected error", result.ErrorMessage);
    }

    #endregion
}
