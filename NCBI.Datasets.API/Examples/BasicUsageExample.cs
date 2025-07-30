using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCBI.Datasets.API.Extensions;
using NCBI.Datasets.API.Models.Enums;

namespace NCBI.Datasets.API.Examples;

/// <summary>
/// Example demonstrating basic usage of the NCBI Datasets API client
/// </summary>
public class BasicUsageExample
{
    /// <summary>
    /// Demonstrates basic API usage
    /// </summary>
    /// <returns>Task</returns>
    public static async Task RunExampleAsync()
    {
        // Setup configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(builder => 
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        services.AddNCBIDatasetsApi(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        using var client = serviceProvider.GetRequiredService<INCBIDatasetsClient>();

        Console.WriteLine("NCBI Datasets API Client Example");
        Console.WriteLine("================================");

        // Example 1: Get download summary for a single genome
        await GetSingleGenomeSummaryExample(client);

        // Example 2: Get download summary for multiple genomes
        await GetMultipleGenomesSummaryExample(client);

        // Example 3: Get download summary with specific chromosomes and annotations
        await GetGenomeSummaryWithOptionsExample(client);

        // Example 4: Download a small genome dataset
        await DownloadGenomeDatasetExample(client);

        Console.WriteLine("\nExample completed successfully!");
    }

    /// <summary>
    /// Example 1: Get download summary for a single genome
    /// </summary>
    private static async Task GetSingleGenomeSummaryExample(INCBIDatasetsClient client)
    {
        Console.WriteLine("\n1. Getting download summary for human genome...");
        
        var humanAccession = "GCF_000001405.40"; // Human reference genome
        var response = await client.GetGenomeDownloadSummaryAsync(humanAccession);

        if (response.IsSuccess && response.Data != null)
        {
            Console.WriteLine($"   ✓ Success! Found {response.Data.RecordCount} record(s)");
            Console.WriteLine($"   ✓ Hydrated download size: {response.Data.Hydrated?.EstimatedFileSizeMb} MB");
            Console.WriteLine($"   ✓ Dehydrated download size: {response.Data.Dehydrated?.EstimatedFileSizeMb} MB");
            
            if (response.Data.AvailableFiles != null)
            {
                Console.WriteLine("   ✓ Available files:");
                if (response.Data.AvailableFiles.AllGenomicFasta != null)
                    Console.WriteLine($"     - Genomic FASTA: {response.Data.AvailableFiles.AllGenomicFasta.FileCount} files, {response.Data.AvailableFiles.AllGenomicFasta.SizeMb:F2} MB");
                if (response.Data.AvailableFiles.ProtFasta != null)
                    Console.WriteLine($"     - Protein FASTA: {response.Data.AvailableFiles.ProtFasta.FileCount} files, {response.Data.AvailableFiles.ProtFasta.SizeMb:F2} MB");
            }
        }
        else
        {
            Console.WriteLine($"   ✗ Failed: {response.ErrorMessage}");
        }
    }

    /// <summary>
    /// Example 2: Get download summary for multiple genomes
    /// </summary>
    private static async Task GetMultipleGenomesSummaryExample(INCBIDatasetsClient client)
    {
        Console.WriteLine("\n2. Getting download summary for multiple genomes...");
        
        var accessions = new[] 
        { 
            "GCF_000001405.40", // Human
            "GCF_000001635.27", // Mouse
            "GCF_000005825.2"   // E. coli
        };

        var response = await client.GetGenomeDownloadSummaryAsync(accessions);

        if (response.IsSuccess && response.Data != null)
        {
            Console.WriteLine($"   ✓ Success! Found {response.Data.RecordCount} record(s)");
            Console.WriteLine($"   ✓ Total hydrated download size: {response.Data.Hydrated?.EstimatedFileSizeMb} MB");
            Console.WriteLine($"   ✓ Total dehydrated download size: {response.Data.Dehydrated?.EstimatedFileSizeMb} MB");
        }
        else
        {
            Console.WriteLine($"   ✗ Failed: {response.ErrorMessage}");
        }
    }

    /// <summary>
    /// Example 3: Get download summary with specific chromosomes and annotations
    /// </summary>
    private static async Task GetGenomeSummaryWithOptionsExample(INCBIDatasetsClient client)
    {
        Console.WriteLine("\n3. Getting download summary with specific options...");
        
        var accession = "GCF_000001405.40"; // Human reference genome
        var chromosomes = new[] { "1", "2", "X", "Y", "MT" }; // Specific chromosomes
        var annotationTypes = new[] 
        { 
            AnnotationForAssemblyType.GENOME_FASTA,
            AnnotationForAssemblyType.PROT_FASTA,
            AnnotationForAssemblyType.RNA_FASTA,
            AnnotationForAssemblyType.GENOME_GFF
        };

        var response = await client.GetGenomeDownloadSummaryAsync(
            accession, 
            chromosomes, 
            annotationTypes);

        if (response.IsSuccess && response.Data != null)
        {
            Console.WriteLine($"   ✓ Success! Found {response.Data.RecordCount} record(s)");
            Console.WriteLine($"   ✓ Filtered download size: {response.Data.Hydrated?.EstimatedFileSizeMb} MB");
            Console.WriteLine($"   ✓ Requested chromosomes: {string.Join(", ", chromosomes)}");
            Console.WriteLine($"   ✓ Requested annotations: {annotationTypes.Length} types");
        }
        else
        {
            Console.WriteLine($"   ✗ Failed: {response.ErrorMessage}");
        }
    }

    /// <summary>
    /// Example 4: Download a small genome dataset
    /// </summary>
    private static async Task DownloadGenomeDatasetExample(INCBIDatasetsClient client)
    {
        Console.WriteLine("\n4. Downloading a small genome dataset...");
        
        // Use a small genome for demonstration
        var accession = "GCF_000005825.2"; // E. coli K-12 MG1655 (small genome)
        
        // First get the download summary
        var summaryResponse = await client.GetGenomeDownloadSummaryAsync(accession);
        
        if (!summaryResponse.IsSuccess || summaryResponse.Data?.Hydrated?.Url == null)
        {
            Console.WriteLine($"   ✗ Failed to get download URL: {summaryResponse.ErrorMessage}");
            return;
        }

        Console.WriteLine($"   ✓ Download URL obtained, estimated size: {summaryResponse.Data.Hydrated.EstimatedFileSizeMb} MB");
        
        // Download the dataset (only if it's reasonably small)
        if (summaryResponse.Data.Hydrated.EstimatedFileSizeMb < 50) // Less than 50MB
        {
            var downloadResponse = await client.DownloadGenomeDatasetAsync(summaryResponse.Data.Hydrated.Url);
            
            if (downloadResponse.IsSuccess && downloadResponse.Data != null)
            {
                Console.WriteLine($"   ✓ Download successful! Size: {downloadResponse.Data.Length:N0} bytes");
                Console.WriteLine($"   ✓ Data could be saved to file or processed further");
                
                // Example: Save to file (commented out to avoid creating files in example)
                // await File.WriteAllBytesAsync($"{accession}_dataset.zip", downloadResponse.Data);
            }
            else
            {
                Console.WriteLine($"   ✗ Download failed: {downloadResponse.ErrorMessage}");
            }
        }
        else
        {
            Console.WriteLine($"   ⚠ Skipping download - file too large for example ({summaryResponse.Data.Hydrated.EstimatedFileSizeMb} MB)");
        }
    }
}
