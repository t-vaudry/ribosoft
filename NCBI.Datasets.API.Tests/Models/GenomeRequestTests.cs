using NCBI.Datasets.API.Models.Enums;
using NCBI.Datasets.API.Models.Genome;
using Xunit;

namespace NCBI.Datasets.API.Tests.Models;

public class GenomeRequestTests
{
    [Fact]
    public void GenomeDownloadSummaryRequest_IsValid_WithValidAccessions_ReturnsTrue()
    {
        // Arrange
        var request = new GenomeDownloadSummaryRequest
        {
            Accessions = new List<string> { "GCF_000001405.40", "GCF_000001635.27" }
        };

        // Act
        var result = request.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GenomeDownloadSummaryRequest_IsValid_WithEmptyAccessions_ReturnsFalse()
    {
        // Arrange
        var request = new GenomeDownloadSummaryRequest
        {
            Accessions = new List<string>()
        };

        // Act
        var result = request.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenomeDownloadSummaryRequest_IsValid_WithNullOrWhitespaceAccessions_ReturnsFalse()
    {
        // Arrange
        var request = new GenomeDownloadSummaryRequest
        {
            Accessions = new List<string> { "GCF_000001405.40", "", "   ", null! }
        };

        // Act
        var result = request.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenomeDownloadSummaryRequest_WithOptionalParameters_IsValid()
    {
        // Arrange
        var request = new GenomeDownloadSummaryRequest
        {
            Accessions = new List<string> { "GCF_000001405.40" },
            Chromosomes = new List<string> { "1", "2", "X", "Y" },
            IncludeAnnotationTypes = new List<AnnotationForAssemblyType> 
            { 
                AnnotationForAssemblyType.GENOME_FASTA, 
                AnnotationForAssemblyType.PROT_FASTA 
            }
        };

        // Act
        var result = request.IsValid();

        // Assert
        Assert.True(result);
        Assert.Equal(4, request.Chromosomes!.Count);
        Assert.Equal(2, request.IncludeAnnotationTypes!.Count);
    }

    [Fact]
    public void GenomeDownloadSummaryPostRequest_IsValid_WithValidAccessions_ReturnsTrue()
    {
        // Arrange
        var request = new GenomeDownloadSummaryPostRequest
        {
            Accessions = new List<string> { "GCF_000001405.40", "GCF_000001635.27" }
        };

        // Act
        var result = request.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GenomeDownloadSummaryPostRequest_IsValid_WithEmptyAccessions_ReturnsFalse()
    {
        // Arrange
        var request = new GenomeDownloadSummaryPostRequest
        {
            Accessions = new List<string>()
        };

        // Act
        var result = request.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AssemblyAccessions_DefaultConstructor_InitializesEmptyList()
    {
        // Arrange & Act
        var assemblyAccessions = new AssemblyAccessions();

        // Assert
        Assert.NotNull(assemblyAccessions.Accessions);
        Assert.Empty(assemblyAccessions.Accessions);
    }

    [Fact]
    public void AssemblyAccessions_CanSetAccessions()
    {
        // Arrange
        var accessions = new List<string> { "GCF_000001405.40", "GCF_000001635.27" };
        var assemblyAccessions = new AssemblyAccessions();

        // Act
        assemblyAccessions.Accessions = accessions;

        // Assert
        Assert.Equal(accessions, assemblyAccessions.Accessions);
        Assert.Equal(2, assemblyAccessions.Accessions.Count);
    }
}
