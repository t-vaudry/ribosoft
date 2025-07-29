using System.Text.Json;
using NCBI.Datasets.API.Models.Genome;
using Xunit;

namespace NCBI.Datasets.API.Tests.Integration;

public class JsonDeserializationTests
{
    [Fact]
    public void AssemblyStats_CanDeserializeStringNumbers()
    {
        // Arrange - This simulates the problematic JSON from NCBI API
        var json = """
        {
            "reports": [
                {
                    "assembly_stats": {
                        "total_sequence_length": "3088269832",
                        "total_ungapped_length": "2881033286",
                        "number_of_contigs": "455",
                        "number_of_scaffolds": "25",
                        "contig_n50": "57879411",
                        "scaffold_n50": "154259566",
                        "gc_percent": "41.0"
                    }
                }
            ]
        }
        """;

        // Act - This should not throw an exception
        var result = JsonSerializer.Deserialize<AssemblyDatasetReport>(json);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Reports);
        Assert.Single(result.Reports);
        
        var stats = result.Reports[0].AssemblyStats;
        Assert.NotNull(stats);
        Assert.Equal(3088269832L, stats.TotalSequenceLength);
        Assert.Equal(2881033286L, stats.TotalUngappedLength);
        Assert.Equal(455, stats.NumberOfContigs);
        Assert.Equal(25, stats.NumberOfScaffolds);
        Assert.Equal(57879411L, stats.ContigN50);
        Assert.Equal(154259566L, stats.ScaffoldN50);
        Assert.Equal(41.0f, stats.GcPercent, precision: 1);
    }

    [Fact]
    public void AssemblyStats_CanDeserializeNumericNumbers()
    {
        // Arrange - This tests normal numeric JSON
        var json = """
        {
            "reports": [
                {
                    "assembly_stats": {
                        "total_sequence_length": 3088269832,
                        "total_ungapped_length": 2881033286,
                        "number_of_contigs": 455,
                        "number_of_scaffolds": 25,
                        "contig_n50": 57879411,
                        "scaffold_n50": 154259566,
                        "gc_percent": 41.0
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<AssemblyDatasetReport>(json);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Reports);
        Assert.Single(result.Reports);
        
        var stats = result.Reports[0].AssemblyStats;
        Assert.NotNull(stats);
        Assert.Equal(3088269832L, stats.TotalSequenceLength);
        Assert.Equal(2881033286L, stats.TotalUngappedLength);
        Assert.Equal(455, stats.NumberOfContigs);
        Assert.Equal(25, stats.NumberOfScaffolds);
        Assert.Equal(57879411L, stats.ContigN50);
        Assert.Equal(154259566L, stats.ScaffoldN50);
        Assert.Equal(41.0f, stats.GcPercent, precision: 1);
    }

    [Fact]
    public void AssemblyStats_HandlesNullAndEmptyValues()
    {
        // Arrange - This tests edge cases
        var json = """
        {
            "reports": [
                {
                    "assembly_stats": {
                        "total_sequence_length": null,
                        "total_ungapped_length": "",
                        "number_of_contigs": "0",
                        "number_of_scaffolds": 0,
                        "contig_n50": null,
                        "scaffold_n50": "",
                        "gc_percent": "0.0"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<AssemblyDatasetReport>(json);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Reports);
        Assert.Single(result.Reports);
        
        var stats = result.Reports[0].AssemblyStats;
        Assert.NotNull(stats);
        Assert.Equal(0L, stats.TotalSequenceLength);
        Assert.Equal(0L, stats.TotalUngappedLength);
        Assert.Equal(0, stats.NumberOfContigs);
        Assert.Equal(0, stats.NumberOfScaffolds);
        Assert.Equal(0L, stats.ContigN50);
        Assert.Equal(0L, stats.ScaffoldN50);
        Assert.Equal(0.0f, stats.GcPercent);
    }
}
