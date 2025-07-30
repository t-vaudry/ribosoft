using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCBI.Datasets.API.Extensions;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace NCBI.Datasets.API.Tests.Integration;

public class TaxonomyAssemblyReportsTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly INCBIDatasetsClient _client;
    private readonly ITestOutputHelper _output;

    public TaxonomyAssemblyReportsTests(ITestOutputHelper output)
    {
        _output = output;
        
        // Setup configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
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
    public async Task GetAssemblyDatasetReportsByTaxonAsync_WithEColiTaxon_ReturnsValidData()
    {
        // Arrange - E. coli K-12 MG1655 taxonomy ID
        var taxonIds = new[] { 511145 };

        // Act - Request complete data
        var result = await _client.GetAssemblyDatasetReportsByTaxonAsync(
            taxonIds, 
            5, 
            NCBI.Datasets.API.Models.Enums.AssemblyDatasetReportsRequestContentType.COMPLETE);

        // Assert
        Assert.True(result.IsSuccess, $"API call failed: {result.ErrorMessage}");
        Assert.NotNull(result.Data);
        
        _output.WriteLine($"Total count: {result.Data.TotalCount}");
        _output.WriteLine($"Reports count: {result.Data.Reports?.Count ?? 0}");

        if (result.Data.Reports != null && result.Data.Reports.Any())
        {
            var firstReport = result.Data.Reports.First();
            _output.WriteLine($"First report accession: {firstReport.Accession}");
            _output.WriteLine($"Organism name: '{firstReport.OrganismName}'");
            _output.WriteLine($"Assembly name: '{firstReport.AssemblyName}'");
            _output.WriteLine($"Assembly level: '{firstReport.AssemblyLevel}'");
            _output.WriteLine($"Submission date: {firstReport.SubmissionDate}");
            _output.WriteLine($"Release date: {firstReport.ReleaseDate}");
            _output.WriteLine($"Taxid: {firstReport.Taxid}");
            
            if (firstReport.AssemblyStats != null)
            {
                _output.WriteLine($"Total sequence length: {firstReport.AssemblyStats.TotalSequenceLength}");
                _output.WriteLine($"Number of contigs: {firstReport.AssemblyStats.NumberOfContigs}");
            }
            else
            {
                _output.WriteLine("Assembly stats is null");
            }

            // Serialize the first report to see the full structure
            var json = JsonSerializer.Serialize(firstReport, new JsonSerializerOptions { WriteIndented = true });
            _output.WriteLine($"Full first report JSON:\n{json}");
        }
        else
        {
            _output.WriteLine("No reports returned");
        }
    }

    [Fact]
    public async Task GetAssemblyDatasetReportsByTaxonAsync_WithHumanTaxon_ReturnsValidData()
    {
        // Arrange - Human taxonomy ID
        var taxonIds = new[] { 9606 };

        // Act - Request complete data
        var result = await _client.GetAssemblyDatasetReportsByTaxonAsync(
            taxonIds, 
            3, 
            NCBI.Datasets.API.Models.Enums.AssemblyDatasetReportsRequestContentType.COMPLETE);

        // Assert
        Assert.True(result.IsSuccess, $"API call failed: {result.ErrorMessage}");
        Assert.NotNull(result.Data);
        
        _output.WriteLine($"Total count: {result.Data.TotalCount}");
        _output.WriteLine($"Reports count: {result.Data.Reports?.Count ?? 0}");

        if (result.Data.Reports != null && result.Data.Reports.Any())
        {
            foreach (var report in result.Data.Reports.Take(3))
            {
                _output.WriteLine($"Accession: {report.Accession}");
                _output.WriteLine($"  Organism: '{report.OrganismName}'");
                _output.WriteLine($"  Assembly: '{report.AssemblyName}'");
                _output.WriteLine($"  Level: '{report.AssemblyLevel}'");
                _output.WriteLine($"  Submitted: {report.SubmissionDate}");
                _output.WriteLine($"  Stats: {(report.AssemblyStats != null ? "Present" : "Null")}");
                _output.WriteLine("");
            }
        }
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
