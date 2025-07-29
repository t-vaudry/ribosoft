using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCBI.Datasets.API.Configuration;
using NCBI.Datasets.API.Services;

namespace NCBI.Datasets.API.Extensions;

/// <summary>
/// Extension methods for service collection configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds NCBI Datasets API services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddNCBIDatasetsApi(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Configure options
        services.Configure<NCBIDatasetsApiOptions>(
            configuration.GetSection(NCBIDatasetsApiOptions.SectionName));

        // Validate configuration
        services.AddSingleton<IValidateOptions<NCBIDatasetsApiOptions>, NCBIDatasetsApiOptionsValidator>();

        // Add HTTP client with proper configuration
        services.AddHttpClient<NCBIDatasetsHttpClient>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<NCBIDatasetsApiOptions>>().Value;
            
            // Ensure BaseUrl ends with a trailing slash for proper URL combination
            var baseUrl = options.BaseUrl.TrimEnd('/') + "/";
            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            
            // Add API key header
            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("api-key", options.ApiKey);
            }

            // Add common headers
            httpClient.DefaultRequestHeaders.Add("User-Agent", "NCBI.Datasets.API.Client/1.0.0");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        
        services.AddScoped<INCBIDatasetsHttpClient>(provider => provider.GetRequiredService<NCBIDatasetsHttpClient>());

        // Add regular HTTP client for newer services
        services.AddHttpClient();

        // Add all API services
        services.AddScoped<GenomeService>();
        services.AddScoped<GeneService>();
        services.AddScoped<VirusService>();
        services.AddScoped<TaxonomyService>();
        services.AddScoped<ProteinService>();
        services.AddScoped<OrganelleService>();
        services.AddScoped<BioSampleService>();
        
        // Add main client
        services.AddScoped<INCBIDatasetsClient, NCBIDatasetsClient>();

        return services;
    }

    /// <summary>
    /// Adds NCBI Datasets API services with custom configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddNCBIDatasetsApi(
        this IServiceCollection services,
        Action<NCBIDatasetsApiOptions> configureOptions)
    {
        // Configure options
        services.Configure(configureOptions);

        // Validate configuration
        services.AddSingleton<IValidateOptions<NCBIDatasetsApiOptions>, NCBIDatasetsApiOptionsValidator>();

        // Add HTTP client with proper configuration
        services.AddHttpClient<NCBIDatasetsHttpClient>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<NCBIDatasetsApiOptions>>().Value;
            
            // Ensure BaseUrl ends with a trailing slash for proper URL combination
            var baseUrl = options.BaseUrl.TrimEnd('/') + "/";
            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            
            // Add API key header
            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("api-key", options.ApiKey);
            }

            // Add common headers
            httpClient.DefaultRequestHeaders.Add("User-Agent", "NCBI.Datasets.API.Client/1.0.0");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        
        services.AddScoped<INCBIDatasetsHttpClient>(provider => provider.GetRequiredService<NCBIDatasetsHttpClient>());

        // Add regular HTTP client for newer services
        services.AddHttpClient();

        // Add all API services
        services.AddScoped<GenomeService>();
        services.AddScoped<GeneService>();
        services.AddScoped<VirusService>();
        services.AddScoped<TaxonomyService>();
        services.AddScoped<ProteinService>();
        services.AddScoped<OrganelleService>();
        services.AddScoped<BioSampleService>();
        
        // Add main client
        services.AddScoped<INCBIDatasetsClient, NCBIDatasetsClient>();

        return services;
    }
}

/// <summary>
/// Validator for NCBI Datasets API options
/// </summary>
public class NCBIDatasetsApiOptionsValidator : IValidateOptions<NCBIDatasetsApiOptions>
{
    /// <summary>
    /// Validates the options
    /// </summary>
    /// <param name="name">Options name</param>
    /// <param name="options">Options to validate</param>
    /// <returns>Validation result</returns>
    public ValidateOptionsResult Validate(string? name, NCBIDatasetsApiOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("NCBIDatasetsApiOptions cannot be null");
        }

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            errors.Add("BaseUrl is required");
        }
        else if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
        {
            errors.Add("BaseUrl must be a valid absolute URI");
        }

        // API key is optional for NCBI Datasets API
        // if (string.IsNullOrWhiteSpace(options.ApiKey))
        // {
        //     errors.Add("ApiKey is required");
        // }

        if (options.TimeoutSeconds <= 0)
        {
            errors.Add("TimeoutSeconds must be greater than 0");
        }

        if (options.MaxRetryAttempts < 0)
        {
            errors.Add("MaxRetryAttempts must be greater than or equal to 0");
        }

        if (options.RetryDelaySeconds < 0)
        {
            errors.Add("RetryDelaySeconds must be greater than or equal to 0");
        }

        return errors.Count > 0 
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
