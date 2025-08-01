using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using NLog;
using NLog.Web;
using Ribosoft.Configuration;
using Ribosoft.Data;
using Ribosoft.Models;
using Ribosoft.Services;
using Ribosoft.Middleware;
using Hangfire.Logging.LogProviders;
using System.Diagnostics.CodeAnalysis;
using NCBI.Datasets.API.Extensions;

[ExcludeFromCodeCoverage]
public class Program
{
    public static async Task Main(string[] args)
    {
        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

        try
        {
            logger.Debug("Starting Ribosoft .NET 8 application");

            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel to use configuration (including environment variables)
            builder.WebHost.UseKestrel((context, serverOptions) =>
            {
                serverOptions.Configure(context.Configuration.GetSection("Kestrel"));
            });

            // Configure services
            ConfigureServices(builder.Services, builder.Configuration);

            // Configure logging
            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            var app = builder.Build();

            // Initialize database (including migrations) FIRST
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await ApplicationDbInitializer.Initialize(services);
            }

            // NOW configure NLog with database logging (after migrations are complete)
            NLogConfiguration.Configure(builder.Configuration);

            // Get a new logger instance that can use database logging
            logger = LogManager.GetCurrentClassLogger();
            logger.Info("Database migrations completed successfully, database logging now active");

            // Configure pipeline
            ConfigurePipeline(app);

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Stopped program because of exception");
            throw;
        }
        finally
        {
            NLog.LogManager.Shutdown();
        }
    }
    
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Database configuration
        var providerName = configuration.GetValue("EntityFrameworkProvider", "SqlServer");
        var connectionString = configuration.GetConnectionString($"{providerName}Connection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty.");
        }

        if (providerName == "Npgsql")
        {
            services.AddDbContext<NpgsqlDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Register ApplicationDbContext to resolve to NpgsqlDbContext
            services.AddScoped<ApplicationDbContext>(provider => provider.GetRequiredService<NpgsqlDbContext>());

            services.AddHangfire(x => x
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(connectionString);
                }, new PostgreSqlStorageOptions
                {
                    InvisibilityTimeout = TimeSpan.FromDays(1)
                }));
        }
        else if (providerName == "SqlServer")
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddHangfire(x => x
                .UseSqlServerStorage(connectionString));
        }
        else
        {
            throw new ArgumentException("Entity Framework provider not supported (use SqlServer or Npgsql).");
        }

        // Identity configuration
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Application services
        services.AddTransient<IEmailSender, MailgunEmailSender>();
        services.AddTransient<ISecureEmailTemplateService, SecureEmailTemplateService>();
        services.AddScoped<IOneTimeCodeService, OneTimeCodeService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        
        // NCBI Datasets API services
        services.AddNCBIDatasetsApi(configuration);
        services.AddScoped<IDatasetDownloadService, DatasetDownloadService>();

        // Localization
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        // Developer page exception filter
        services.AddDatabaseDeveloperPageExceptionFilter();

        // MVC with authorization and localization
        services.AddControllersWithViews(config =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            config.Filters.Add(new AuthorizeFilter(policy));
        })
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

        // Pagination
        services.AddCloudscribePagination();

        // Health checks
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        // HSTS configuration for production
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });

        // HTTPS redirection
        services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
            options.HttpsPort = configuration.GetValue<int?>("HTTPS_PORT") ?? 
                               configuration.GetValue<int?>("ASPNETCORE_HTTPS_PORT");
        });

        // Hangfire server
        services.AddHangfireServer(options =>
        {
            options.Queues = new[] { "default", "blast", "downloads", "downloads-high", "downloads-low" };
        });
    }
    
    private static void ConfigurePipeline(WebApplication app)
    {
        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // Enable HSTS (HTTP Strict Transport Security) in production
            app.UseHsts();
        }
        
        // Enable HTTPS redirection
        app.UseHttpsRedirection();
        
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        
        // Request logging middleware (after authentication so we have user info)
        app.UseRequestLogging();
        
        // Hangfire dashboard
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new Ribosoft.DashboardAuthorizationFilter() }
        });
        
        // Configure routes
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // Health check endpoint
        app.MapHealthChecks("/health");

        // Validate download directory configuration
        var (isValid, errorMessage) = Ribosoft.Jobs.DatasetDownloadJob.ValidateDownloadConfiguration(app.Configuration);
        if (!isValid)
        {
            Console.WriteLine($"FATAL: Download directory configuration error: {errorMessage}");
            Console.WriteLine("Application cannot start. Please fix the download directory permissions and try again.");
            throw new InvalidOperationException($"Download directory configuration error: {errorMessage}");
        }
        else
        {
            var downloadDir = app.Configuration["Assemblies:Path"] ?? 
                             Path.Combine(Directory.GetCurrentDirectory(), "Downloads", "Datasets");
            Console.WriteLine($"INFO: Download directory validated successfully: {downloadDir}");
        }
    }
}
