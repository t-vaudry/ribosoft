using Microsoft.Extensions.Logging;
using Moq;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Download;
using NCBI.Datasets.API.Models.Virus;
using NCBI.Datasets.API.Models.Enums;
using NCBI.Datasets.API.Services;
using Xunit;

namespace NCBI.Datasets.API.Tests.Services;

public class VirusServiceTests
{
    private readonly Mock<INCBIDatasetsHttpClient> _mockHttpClient;
    private readonly Mock<ILogger<VirusService>> _mockLogger;
    private readonly VirusService _virusService;

    public VirusServiceTests()
    {
        _mockHttpClient = new Mock<INCBIDatasetsHttpClient>();
        _mockLogger = new Mock<ILogger<VirusService>>();
        _virusService = new VirusService(_mockHttpClient.Object, _mockLogger.Object);
    }

    #region Virus Genome Summary Tests

    [Fact]
    public async Task GetVirusGenomeSummaryByTaxonAsync_WithValidTaxon_ReturnsSuccess()
    {
        // Arrange
        var taxon = "2697049"; // SARS-CoV-2
        var expectedResponse = new DownloadSummary
        {
            RecordCount = 100,
            Hydrated = new DownloadSummaryHydrated
            {
                EstimatedFileSizeMb = 50,
                Url = "https://example.com/download"
            }
        };
        var apiResponse = ApiResponse<DownloadSummary>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<DownloadSummary>("/virus/taxon/2697049/genome", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.GetVirusGenomeSummaryByTaxonAsync(taxon);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(100, result.Data.RecordCount);
        Assert.Equal(50, result.Data.Hydrated!.EstimatedFileSizeMb);
    }

    [Fact]
    public async Task GetVirusGenomeSummaryByTaxonAsync_WithFilters_BuildsCorrectUrl()
    {
        // Arrange
        var taxon = "2697049";
        var refSeqOnly = true;
        var completeOnly = true;
        
        var expectedResponse = new DownloadSummary { RecordCount = 50 };
        var apiResponse = ApiResponse<DownloadSummary>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<DownloadSummary>("/virus/taxon/2697049/genome?refseq_only=true&complete_only=true", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.GetVirusGenomeSummaryByTaxonAsync(taxon, refSeqOnly, completeOnly);

        // Assert
        Assert.True(result.IsSuccess);
        _mockHttpClient.Verify(x => x.GetAsync<DownloadSummary>(
            "/virus/taxon/2697049/genome?refseq_only=true&complete_only=true", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVirusGenomeSummaryByTaxonAsync_WithNullTaxon_ReturnsError()
    {
        // Act
        var result = await _virusService.GetVirusGenomeSummaryByTaxonAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    [Fact]
    public async Task GetVirusGenomeSummaryAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new VirusDatasetRequest
        {
            Taxon = "2697049",
            RefSeqOnly = true,
            Filter = new VirusDatasetFilter
            {
                CompleteOnly = true,
                Host = "Homo sapiens"
            }
        };
        var expectedResponse = new DownloadSummary { RecordCount = 25 };
        var apiResponse = ApiResponse<DownloadSummary>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<VirusDatasetRequest, DownloadSummary>("/virus/genome", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.GetVirusGenomeSummaryAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(25, result.Data.RecordCount);
    }

    [Fact]
    public async Task GetVirusGenomeSummaryAsync_WithNullRequest_ReturnsError()
    {
        // Act
        var result = await _virusService.GetVirusGenomeSummaryAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null", result.ErrorMessage);
    }

    #endregion

    #region SARS-CoV-2 Protein Summary Tests

    [Fact]
    public async Task GetSars2ProteinSummaryAsync_WithValidProteins_ReturnsSuccess()
    {
        // Arrange
        var proteins = new[] { "spike", "nucleocapsid" };
        var expectedResponse = new Sars2ProteinDatasetSummary
        {
            Proteins = new List<Sars2ProteinSummary>
            {
                new Sars2ProteinSummary
                {
                    ProteinName = "spike",
                    SequenceCount = 1000,
                    RefSeqCount = 100,
                    GenbankCount = 900
                }
            },
            TotalCount = 1000
        };
        var apiResponse = ApiResponse<Sars2ProteinDatasetSummary>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<Sars2ProteinDatasetSummary>("/virus/taxon/sars2/protein/spike,nucleocapsid", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.GetSars2ProteinSummaryAsync(proteins);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1000, result.Data.TotalCount);
        Assert.Single(result.Data.Proteins);
        Assert.Equal("spike", result.Data.Proteins[0].ProteinName);
    }

    [Fact]
    public async Task GetSars2ProteinSummaryAsync_WithRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new Sars2ProteinDatasetRequest
        {
            Proteins = new List<string> { "spike" },
            RefSeqOnly = true
        };
        var expectedResponse = new Sars2ProteinDatasetSummary { TotalCount = 100 };
        var apiResponse = ApiResponse<Sars2ProteinDatasetSummary>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<Sars2ProteinDatasetRequest, Sars2ProteinDatasetSummary>("/virus/taxon/sars2/protein", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.GetSars2ProteinSummaryAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(100, result.Data.TotalCount);
    }

    [Fact]
    public async Task GetSars2ProteinSummaryAsync_WithNullProteins_ReturnsError()
    {
        // Act
        var result = await _virusService.GetSars2ProteinSummaryAsync((IEnumerable<string>)null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Virus Genome Table Tests

    [Fact]
    public async Task GetVirusGenomeTableByTaxonAsync_WithValidTaxon_ReturnsSuccess()
    {
        // Arrange
        var taxon = "2697049";
        var format = "tsv";
        var tableFields = new[] { "accession", "virus-name", "length" };
        var expectedResponse = "accession\tvirus-name\tlength\nNC_045512.2\tSARS-CoV-2\t29903";
        var apiResponse = ApiResponse<string>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<string>("/virus/taxon/2697049/genome/table?format=tsv&table_fields=accession&table_fields=virus-name&table_fields=length", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.GetVirusGenomeTableByTaxonAsync(taxon, format, null, null, tableFields);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Contains("NC_045512.2", result.Data);
        Assert.Contains("SARS-CoV-2", result.Data);
    }

    [Fact]
    public async Task GetVirusGenomeTableByTaxonAsync_WithNullTaxon_ReturnsError()
    {
        // Act
        var result = await _virusService.GetVirusGenomeTableByTaxonAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region SARS-CoV-2 Protein Table Tests

    [Fact]
    public async Task GetSars2ProteinTableAsync_WithValidProteins_ReturnsSuccess()
    {
        // Arrange
        var proteins = new[] { "spike" };
        var format = "csv";
        var expectedResponse = "accession,protein-name,length\nYP_009724390.1,spike,1273";
        var apiResponse = ApiResponse<string>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<string>("/virus/taxon/sars2/protein/spike/table?format=csv", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.GetSars2ProteinTableAsync(proteins, format);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Contains("YP_009724390.1", result.Data);
        Assert.Contains("spike", result.Data);
    }

    [Fact]
    public async Task GetSars2ProteinTableAsync_WithNullProteins_ReturnsError()
    {
        // Act
        var result = await _virusService.GetSars2ProteinTableAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Virus Dataset Reports Tests

    [Fact]
    public async Task GetVirusDatasetReportsByTaxonAsync_WithValidTaxon_ReturnsSuccess()
    {
        // Arrange
        var taxon = "2697049";
        var expectedResponse = new VirusDataReportPage
        {
            Reports = new List<VirusReportMatch>
            {
                new VirusReportMatch
                {
                    Virus = new VirusDescriptor
                    {
                        Accession = "NC_045512.2",
                        VirusName = "Severe acute respiratory syndrome coronavirus 2",
                        Length = 29903,
                        IsComplete = true,
                        IsAnnotated = true
                    }
                }
            },
            TotalCount = 1
        };
        var apiResponse = ApiResponse<VirusDataReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<VirusDataReportPage>("/virus/taxon/2697049/dataset_report", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.GetVirusDatasetReportsByTaxonAsync(taxon);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Reports);
        Assert.Equal("NC_045512.2", result.Data.Reports[0].Virus!.Accession);
        Assert.Equal(29903, result.Data.Reports[0].Virus!.Length);
    }

    [Fact]
    public async Task GetVirusDatasetReportsByAccessionAsync_WithValidAccessions_ReturnsSuccess()
    {
        // Arrange
        var accessions = new[] { "NC_045512.2", "MN908947.3" };
        var expectedResponse = new VirusDataReportPage
        {
            Reports = new List<VirusReportMatch>(),
            TotalCount = 2
        };
        var apiResponse = ApiResponse<VirusDataReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<VirusDataReportPage>("/virus/accession/NC_045512.2,MN908947.3/dataset_report", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.GetVirusDatasetReportsByAccessionAsync(accessions);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.TotalCount);
    }

    [Fact]
    public async Task GetVirusDatasetReportsAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new VirusDataReportRequest
        {
            Filter = new VirusDatasetFilter
            {
                Taxon = "2697049",
                CompleteOnly = true
            },
            ReturnedContent = VirusDataReportRequestContentType.COMPLETE,
            PageSize = 10
        };
        var expectedResponse = new VirusDataReportPage { TotalCount = 100 };
        var apiResponse = ApiResponse<VirusDataReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<VirusDataReportRequest, VirusDataReportPage>("/virus", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.GetVirusDatasetReportsAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(100, result.Data.TotalCount);
    }

    [Fact]
    public async Task GetVirusDatasetReportsByTaxonAsync_WithNullTaxon_ReturnsError()
    {
        // Act
        var result = await _virusService.GetVirusDatasetReportsByTaxonAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Virus Annotation Reports Tests

    [Fact]
    public async Task GetVirusAnnotationReportsByTaxonAsync_WithValidTaxon_ReturnsSuccess()
    {
        // Arrange
        var taxon = "2697049";
        var expectedResponse = new VirusAnnotationReportPage
        {
            Reports = new List<VirusAnnotationReport>
            {
                new VirusAnnotationReport
                {
                    Accession = "NC_045512.2",
                    Features = new List<AnnotationFeature>
                    {
                        new AnnotationFeature
                        {
                            Type = "gene",
                            Name = "S",
                            Start = 21563,
                            End = 25384,
                            Product = "spike glycoprotein"
                        }
                    }
                }
            },
            TotalCount = 1
        };
        var apiResponse = ApiResponse<VirusAnnotationReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<VirusAnnotationReportPage>("/virus/taxon/2697049/annotation_report", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.GetVirusAnnotationReportsByTaxonAsync(taxon);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Reports);
        Assert.Equal("NC_045512.2", result.Data.Reports[0].Accession);
        Assert.Single(result.Data.Reports[0].Features);
        Assert.Equal("S", result.Data.Reports[0].Features[0].Name);
    }

    [Fact]
    public async Task GetVirusAnnotationReportsByAccessionAsync_WithValidAccessions_ReturnsSuccess()
    {
        // Arrange
        var accessions = new[] { "NC_045512.2" };
        var expectedResponse = new VirusAnnotationReportPage { TotalCount = 1 };
        var apiResponse = ApiResponse<VirusAnnotationReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<VirusAnnotationReportPage>("/virus/accession/NC_045512.2/annotation_report", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.GetVirusAnnotationReportsByAccessionAsync(accessions);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TotalCount);
    }

    [Fact]
    public async Task GetVirusAnnotationReportsAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new VirusDataReportRequest
        {
            Filter = new VirusDatasetFilter { Accessions = new List<string> { "NC_045512.2" } }
        };
        var expectedResponse = new VirusAnnotationReportPage { TotalCount = 1 };
        var apiResponse = ApiResponse<VirusAnnotationReportPage>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<VirusDataReportRequest, VirusAnnotationReportPage>("/virus/annotation_report", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.GetVirusAnnotationReportsAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TotalCount);
    }

    #endregion

    #region Virus Availability Tests

    [Fact]
    public async Task CheckVirusAvailabilityByAccessionAsync_WithValidAccessions_ReturnsSuccess()
    {
        // Arrange
        var accessions = new[] { "NC_045512.2", "INVALID123" };
        var expectedResponse = new VirusAvailabilityReply
        {
            ValidAccessions = new List<string> { "NC_045512.2" },
            InvalidAccessions = new List<string> { "INVALID123" },
            Message = "1 valid, 1 invalid accession"
        };
        var apiResponse = ApiResponse<VirusAvailabilityReply>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.GetAsync<VirusAvailabilityReply>("/virus/accession/NC_045512.2,INVALID123/check", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.CheckVirusAvailabilityByAccessionAsync(accessions);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.ValidAccessions);
        Assert.Single(result.Data.InvalidAccessions);
        Assert.Equal("NC_045512.2", result.Data.ValidAccessions[0]);
        Assert.Equal("INVALID123", result.Data.InvalidAccessions[0]);
    }

    [Fact]
    public async Task CheckVirusAvailabilityAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new VirusAvailabilityRequest
        {
            Accessions = new List<string> { "NC_045512.2" }
        };
        var expectedResponse = new VirusAvailabilityReply
        {
            ValidAccessions = new List<string> { "NC_045512.2" },
            InvalidAccessions = new List<string>()
        };
        var apiResponse = ApiResponse<VirusAvailabilityReply>.Success(expectedResponse);

        _mockHttpClient
            .Setup(x => x.PostAsync<VirusAvailabilityRequest, VirusAvailabilityReply>("/virus/check", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.CheckVirusAvailabilityAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.ValidAccessions);
        Assert.Empty(result.Data.InvalidAccessions);
    }

    [Fact]
    public async Task CheckVirusAvailabilityByAccessionAsync_WithNullAccessions_ReturnsError()
    {
        // Act
        var result = await _virusService.CheckVirusAvailabilityByAccessionAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Virus Genome Download Tests

    [Fact]
    public async Task DownloadVirusGenomeDatasetByTaxonAsync_WithValidTaxon_ReturnsSuccess()
    {
        // Arrange
        var taxon = "2697049";
        var annotationTypes = new[] { VirusDatasetReportType.DATASET_REPORT, VirusDatasetReportType.ANNOTATION };
        var filename = "sars_cov2_genomes.zip";
        
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        var apiResponse = ApiResponse<byte[]>.Success(expectedData);

        _mockHttpClient
            .Setup(x => x.DownloadFileAsync(
                "/virus/taxon/2697049/genome/download?include_annotation_type=DATASET_REPORT&include_annotation_type=ANNOTATION&filename=sars_cov2_genomes.zip", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.DownloadVirusGenomeDatasetByTaxonAsync(taxon, annotationTypes, null, null, filename);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedData, result.Data);
    }

    [Fact]
    public async Task DownloadVirusGenomeDatasetByAccessionAsync_WithValidAccessions_ReturnsSuccess()
    {
        // Arrange
        var accessions = new[] { "NC_045512.2" };
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        var apiResponse = ApiResponse<byte[]>.Success(expectedData);

        _mockHttpClient
            .Setup(x => x.DownloadFileAsync("/virus/accession/NC_045512.2/genome/download", It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.DownloadVirusGenomeDatasetByAccessionAsync(accessions);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedData, result.Data);
    }

    [Fact]
    public async Task DownloadVirusGenomeDatasetAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new VirusDatasetRequest
        {
            Accessions = new List<string> { "NC_045512.2" },
            IncludeAnnotationType = new List<VirusAnnotationType> { VirusAnnotationType.GENOME_FASTA }
        };
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        var apiResponse = ApiResponse<byte[]>.Success(expectedData);

        _mockHttpClient
            .Setup(x => x.PostDownloadAsync("/virus/genome/download", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.DownloadVirusGenomeDatasetAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedData, result.Data);
    }

    [Fact]
    public async Task DownloadVirusGenomeDatasetByTaxonAsync_WithNullTaxon_ReturnsError()
    {
        // Act
        var result = await _virusService.DownloadVirusGenomeDatasetByTaxonAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region SARS-CoV-2 Protein Download Tests

    [Fact]
    public async Task DownloadSars2ProteinDatasetAsync_WithValidProteins_ReturnsSuccess()
    {
        // Arrange
        var proteins = new[] { "spike" };
        var annotationTypes = new[] { VirusDatasetReportType.DATASET_REPORT };
        var refSeqOnly = true;
        var filename = "spike_proteins.zip";
        
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        var apiResponse = ApiResponse<byte[]>.Success(expectedData);

        _mockHttpClient
            .Setup(x => x.DownloadFileAsync(
                "/virus/taxon/sars2/protein/spike/download?include_annotation_type=DATASET_REPORT&refseq_only=true&filename=spike_proteins.zip", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.DownloadSars2ProteinDatasetAsync(proteins, annotationTypes, refSeqOnly, filename);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedData, result.Data);
    }

    [Fact]
    public async Task DownloadSars2ProteinDatasetAsync_WithRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new Sars2ProteinDatasetRequest
        {
            Proteins = new List<string> { "spike" },
            RefSeqOnly = true
        };
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        var apiResponse = ApiResponse<byte[]>.Success(expectedData);

        _mockHttpClient
            .Setup(x => x.PostDownloadAsync("/virus/taxon/sars2/protein/download", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _virusService.DownloadSars2ProteinDatasetAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedData, result.Data);
    }

    [Fact]
    public async Task DownloadSars2ProteinDatasetAsync_WithNullProteins_ReturnsError()
    {
        // Act
        var result = await _virusService.DownloadSars2ProteinDatasetAsync((IEnumerable<string>)null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("cannot be null or empty", result.ErrorMessage);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task GetVirusGenomeSummaryByTaxonAsync_WhenHttpClientThrows_ReturnsError()
    {
        // Arrange
        var taxon = "2697049";
        _mockHttpClient
            .Setup(x => x.GetAsync<DownloadSummary>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _virusService.GetVirusGenomeSummaryByTaxonAsync(taxon);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unexpected error", result.ErrorMessage);
    }

    [Fact]
    public async Task DownloadVirusGenomeDatasetByTaxonAsync_WhenHttpClientThrows_ReturnsError()
    {
        // Arrange
        var taxon = "2697049";
        _mockHttpClient
            .Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _virusService.DownloadVirusGenomeDatasetByTaxonAsync(taxon);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unexpected error", result.ErrorMessage);
    }

    #endregion
}
