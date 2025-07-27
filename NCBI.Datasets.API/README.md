# NCBI Datasets API Client Library

A C# client library for the NCBI Datasets v2 REST API, designed for efficient integration with the Ribosoft project and other .NET applications.

## Features

- **Comprehensive API Coverage**: Support for genome download summaries and dataset downloads
- **Async/Await Support**: Full asynchronous programming model
- **Retry Logic**: Built-in retry mechanism with exponential backoff
- **Configuration-Based**: Easy configuration through appsettings.json
- **Dependency Injection**: Full support for .NET dependency injection
- **Type Safety**: Strongly-typed models for all API responses
- **Error Handling**: Comprehensive error handling and logging
- **Unit Tested**: Extensive unit and integration test coverage

## Quick Start

### 1. Installation

Add the project reference to your application:

```xml
<ProjectReference Include="..\NCBI.Datasets.API\NCBI.Datasets.API.csproj" />
```

### 2. Configuration

Add the following to your `appsettings.json`:

```json
{
  "NCBIDatasetsApi": {
    "BaseUrl": "https://api.ncbi.nlm.nih.gov/datasets/v2",
    "ApiKey": "your-api-key-here",
    "TimeoutSeconds": 30,
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 1
  }
}
```

For development, create `appsettings.Development.json` with your API key:

```json
{
  "NCBIDatasetsApi": {
    "ApiKey": "your-development-api-key"
  }
}
```

### 3. Service Registration

Register the services in your `Program.cs` or `Startup.cs`:

```csharp
using NCBI.Datasets.API.Extensions;

// In Program.cs (minimal API)
builder.Services.AddNCBIDatasetsApi(builder.Configuration);

// Or in Startup.cs
services.AddNCBIDatasetsApi(Configuration);
```

### 4. Usage

Inject and use the client in your services:

```csharp
public class MyService
{
    private readonly INCBIDatasetsClient _ncbiClient;

    public MyService(INCBIDatasetsClient ncbiClient)
    {
        _ncbiClient = ncbiClient;
    }

    public async Task<string> GetHumanGenomeInfoAsync()
    {
        var response = await _ncbiClient.GetGenomeDownloadSummaryAsync("GCF_000001405.40");
        
        if (response.IsSuccess)
        {
            return $"Human genome has {response.Data.RecordCount} records, " +
                   $"download size: {response.Data.Hydrated?.EstimatedFileSizeMb} MB";
        }
        
        return $"Error: {response.ErrorMessage}";
    }
}
```

## API Reference

### INCBIDatasetsClient Interface

#### GetGenomeDownloadSummaryAsync

Get download summary for a single genome assembly:

```csharp
var response = await client.GetGenomeDownloadSummaryAsync(
    accession: "GCF_000001405.40",
    chromosomes: new[] { "1", "2", "X", "Y" },
    annotationTypes: new[] { AnnotationForAssemblyType.GENOME_FASTA, AnnotationForAssemblyType.PROT_FASTA }
);
```

Get download summary for multiple genome assemblies:

```csharp
var accessions = new[] { "GCF_000001405.40", "GCF_000001635.27" };
var response = await client.GetGenomeDownloadSummaryAsync(accessions);
```

#### GetGenomeDownloadSummaryByPostAsync

For larger requests (many accessions), use the POST method:

```csharp
var request = new GenomeDownloadSummaryRequest
{
    Accessions = new List<string> { /* many accessions */ },
    Chromosomes = new List<string> { "1", "2" },
    IncludeAnnotationTypes = new List<AnnotationForAssemblyType> 
    { 
        AnnotationForAssemblyType.GENOME_FASTA 
    }
};

var response = await client.GetGenomeDownloadSummaryByPostAsync(request);
```

#### DownloadGenomeDatasetAsync

Download the actual dataset:

```csharp
// First get the download URL
var summaryResponse = await client.GetGenomeDownloadSummaryAsync("GCF_000005825.2");
var downloadUrl = summaryResponse.Data?.Hydrated?.Url;

// Then download the dataset
var downloadResponse = await client.DownloadGenomeDatasetAsync(downloadUrl);
if (downloadResponse.IsSuccess)
{
    await File.WriteAllBytesAsync("dataset.zip", downloadResponse.Data);
}
```

## Models

### DownloadSummary

Main response model containing:
- `RecordCount`: Number of records found
- `Hydrated`: Direct download information
- `Dehydrated`: Dehydrated download information  
- `AvailableFiles`: Information about available file types
- `Errors`: Any errors encountered
- `Messages`: Processing messages

### AnnotationForAssemblyType Enum

Available annotation types:
- `GENOME_FASTA`: Genomic sequences
- `PROT_FASTA`: Protein sequences
- `RNA_FASTA`: RNA sequences
- `GENOME_GFF`: GFF annotation
- `GENOME_GBFF`: GenBank format
- `GENOME_GTF`: GTF annotation
- `CDS_FASTA`: Coding sequences
- `SEQUENCE_REPORT`: Sequence reports

## Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `BaseUrl` | NCBI Datasets API base URL | `https://api.ncbi.nlm.nih.gov/datasets/v2` |
| `ApiKey` | Your NCBI API key | Required |
| `TimeoutSeconds` | HTTP request timeout | 30 |
| `MaxRetryAttempts` | Maximum retry attempts | 3 |
| `RetryDelaySeconds` | Base delay between retries | 1 |

## Error Handling

The library uses the `ApiResponse<T>` wrapper for all responses:

```csharp
var response = await client.GetGenomeDownloadSummaryAsync("GCF_000001405.40");

if (response.IsSuccess)
{
    // Use response.Data
    Console.WriteLine($"Found {response.Data.RecordCount} records");
}
else
{
    // Handle error
    Console.WriteLine($"Error {response.StatusCode}: {response.ErrorMessage}");
}
```

## Logging

The library uses Microsoft.Extensions.Logging. Configure logging levels in your `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "NCBI.Datasets.API": "Information"
    }
  }
}
```

## Testing

Run the unit tests:

```bash
dotnet test NCBI.Datasets.API.Tests
```

Integration tests require a valid API key in `appsettings.Development.json`.

## Examples

See the `Examples/BasicUsageExample.cs` file for comprehensive usage examples.

## Security

- API keys are stored in configuration files
- Development configuration files are excluded from git
- Production API keys should be stored securely (Azure Key Vault, etc.)

## Contributing

1. Follow the existing code style and patterns
2. Add unit tests for new functionality
3. Update documentation for API changes
4. Ensure all tests pass before submitting

## License

This library is part of the Ribosoft project and is licensed under the GNU General Public License v3.0.
