using System.Text.Json;
using System.Text.Json.Serialization;
using NCBI.Datasets.API.Converters;
using Xunit;

namespace NCBI.Datasets.API.Tests.Converters;

public class FlexibleNumberConvertersTests
{
    private readonly JsonSerializerOptions _options;

    public FlexibleNumberConvertersTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new FlexibleLongConverter());
        _options.Converters.Add(new FlexibleIntConverter());
        _options.Converters.Add(new FlexibleFloatConverter());
    }

    [Theory]
    [InlineData("123", 123L)]
    [InlineData("\"456\"", 456L)]
    [InlineData("null", 0L)]
    [InlineData("\"\"", 0L)]
    public void FlexibleLongConverter_CanParseVariousFormats(string json, long expected)
    {
        // Act
        var result = JsonSerializer.Deserialize<long>(json, _options);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", 123)]
    [InlineData("\"456\"", 456)]
    [InlineData("null", 0)]
    [InlineData("\"\"", 0)]
    public void FlexibleIntConverter_CanParseVariousFormats(string json, int expected)
    {
        // Act
        var result = JsonSerializer.Deserialize<int>(json, _options);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123.45", 123.45f)]
    [InlineData("\"456.78\"", 456.78f)]
    [InlineData("null", 0f)]
    [InlineData("\"\"", 0f)]
    public void FlexibleFloatConverter_CanParseVariousFormats(string json, float expected)
    {
        // Act
        var result = JsonSerializer.Deserialize<float>(json, _options);

        // Assert
        Assert.Equal(expected, result, precision: 2);
    }

    [Fact]
    public void FlexibleLongConverter_ThrowsOnInvalidString()
    {
        // Arrange
        var json = "\"not-a-number\"";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<long>(json, _options));
    }

    [Fact]
    public void FlexibleIntConverter_ThrowsOnInvalidString()
    {
        // Arrange
        var json = "\"not-a-number\"";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<int>(json, _options));
    }

    [Fact]
    public void FlexibleFloatConverter_ThrowsOnInvalidString()
    {
        // Arrange
        var json = "\"not-a-number\"";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<float>(json, _options));
    }

    [Fact]
    public void FlexibleConverters_WorkWithComplexObject()
    {
        // Arrange
        var json = """
        {
            "total_sequence_length": "12345678",
            "number_of_contigs": "42",
            "gc_percent": "45.67"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<TestAssemblyStats>(json, _options);

        // Assert
        Assert.Equal(12345678L, result.TotalSequenceLength);
        Assert.Equal(42, result.NumberOfContigs);
        Assert.Equal(45.67f, result.GcPercent, precision: 2);
    }

    private class TestAssemblyStats
    {
        [JsonPropertyName("total_sequence_length")]
        [JsonConverter(typeof(FlexibleLongConverter))]
        public long TotalSequenceLength { get; set; }

        [JsonPropertyName("number_of_contigs")]
        [JsonConverter(typeof(FlexibleIntConverter))]
        public int NumberOfContigs { get; set; }

        [JsonPropertyName("gc_percent")]
        [JsonConverter(typeof(FlexibleFloatConverter))]
        public float GcPercent { get; set; }
    }
}
