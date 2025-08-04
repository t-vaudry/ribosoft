using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCBI.Datasets.API.Extensions;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace NCBI.Datasets.API.Tests.Integration;

public class AccessionBasedReportsTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly INCBIDatasetsClient _client;
    private readonly ITestOutputHelper _output;

    public AccessionBasedReportsTests(ITestOutputHelper output)
    {
        _output = output;
        
        // Setup configuration - include environment variables for CI/CD scenarios
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables() // This will read NCBIDatasetsApi__ApiKey from environment
            .Build();

        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddNCBIDatasetsApi(configuration);

        _serviceProvider = services.BuildServiceProvider();
        _client = _serviceProvider.GetRequiredService<INCBIDatasetsClient>();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAssemblyDatasetReportsAsync_WithEColiAccession_ReturnsCompleteData()
    {
        // Arrange - E. coli K-12 MG1655 reference genome accession
        var accessions = new[] { "GCF_000005825.2" };

        // Act - Request complete data using accession-based endpoint
        var result = await _client.GetAssemblyDatasetReportsAsync(
            accessions, 
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
            _output.WriteLine($"Accession: {firstReport.Accession}");
            _output.WriteLine($"Organism name: '{firstReport.OrganismName}'");
            _output.WriteLine($"Assembly name: '{firstReport.AssemblyName}'");
            _output.WriteLine($"Assembly level: '{firstReport.AssemblyLevel}'");
            _output.WriteLine($"Assembly status: '{firstReport.AssemblyStatus}'");
            _output.WriteLine($"Submission date: {firstReport.SubmissionDate}");
            _output.WriteLine($"Release date: {firstReport.ReleaseDate}");
            _output.WriteLine($"Taxid: {firstReport.Taxid}");
            _output.WriteLine($"BioProject: '{firstReport.BioprojectAccession}'");
            _output.WriteLine($"BioSample: '{firstReport.BiosampleAccession}'");
            
            if (firstReport.AssemblyStats != null)
            {
                _output.WriteLine($"Total sequence length: {firstReport.AssemblyStats.TotalSequenceLength}");
                _output.WriteLine($"Number of contigs: {firstReport.AssemblyStats.NumberOfContigs}");
                _output.WriteLine($"GC percent: {firstReport.AssemblyStats.GcPercent}");
            }
            else
            {
                _output.WriteLine("Assembly stats is null");
            }

            // Check if this endpoint returns complete data
            Assert.False(string.IsNullOrEmpty(firstReport.OrganismName), "Organism name should not be empty");
            Assert.False(string.IsNullOrEmpty(firstReport.AssemblyName), "Assembly name should not be empty");
            Assert.False(string.IsNullOrEmpty(firstReport.AssemblyLevel), "Assembly level should not be empty");
            Assert.True(firstReport.Taxid > 0, "Taxid should be greater than 0");

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
    [Trait("Category", "Integration")]
    public async Task GetAssemblyDatasetReportsAsync_WithMultipleAccessions_ReturnsCompleteData()
    {
        // Arrange - Multiple well-known reference genomes
        var accessions = new[] { "GCF_000005825.2", "GCF_000001405.40" }; // E. coli and Human

        // Act
        var result = await _client.GetAssemblyDatasetReportsAsync(
            accessions, 
            10, 
            NCBI.Datasets.API.Models.Enums.AssemblyDatasetReportsRequestContentType.COMPLETE);

        // Assert
        Assert.True(result.IsSuccess, $"API call failed: {result.ErrorMessage}");
        Assert.NotNull(result.Data);
        
        if (result.Data.Reports != null && result.Data.Reports.Any())
        {
            foreach (var report in result.Data.Reports)
            {
                _output.WriteLine($"Accession: {report.Accession}");
                _output.WriteLine($"  Organism: '{report.OrganismName}'");
                _output.WriteLine($"  Assembly: '{report.AssemblyName}'");
                _output.WriteLine($"  Level: '{report.AssemblyLevel}'");
                _output.WriteLine($"  Status: '{report.AssemblyStatus}'");
                _output.WriteLine($"  Submitted: {report.SubmissionDate}");
                _output.WriteLine($"  Taxid: {report.Taxid}");
                _output.WriteLine("");
                
                // Verify that we get complete data
                Assert.False(string.IsNullOrEmpty(report.OrganismName), $"Organism name should not be empty for {report.Accession}");
                Assert.False(string.IsNullOrEmpty(report.AssemblyName), $"Assembly name should not be empty for {report.Accession}");
            }
        }
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
