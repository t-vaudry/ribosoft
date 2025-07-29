using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using NLog;
using NLog.Web;
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
            
            // Configure NLog database target BEFORE UseNLog() call
            var providerName = builder.Configuration["EntityFrameworkProvider"];
            string? connectionString = null;
            string? dbProvider = null;
            
            if (providerName == "Npgsql")
            {
                connectionString = builder.Configuration.GetConnectionString("NpgsqlConnection");
                dbProvider = "Npgsql.NpgsqlConnection, Npgsql";
            }
            else if (providerName == "SqlServer")
            {
                connectionString = builder.Configuration.GetConnectionString("SqlServerConnection");
                dbProvider = "System.Data.SqlClient.SqlConnection, System.Data.SqlClient";
            }
            
            // Configure NLog programmatically instead of using variables
            if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(dbProvider))
            {
                var config = new NLog.Config.LoggingConfiguration();
                
                // Load existing configuration from file
                var fileConfig = new NLog.Config.XmlLoggingConfiguration("nlog.config");
                
                // Copy existing targets and rules
                foreach (var target in fileConfig.AllTargets)
                {
                    if (target.Name != "database")
                    {
                        config.AddTarget(target);
                    }
                }
                
                foreach (var rule in fileConfig.LoggingRules)
                {
                    if (!rule.Targets.Any(t => t.Name == "database"))
                    {
                        config.LoggingRules.Add(rule);
                    }
                }
                
                // Create database target programmatically
                var databaseTarget = new NLog.Targets.DatabaseTarget("database")
                {
                    ConnectionString = connectionString,
                    DBProvider = dbProvider,
                    CommandText = @"INSERT INTO ""ActivityLogs"" (""Timestamp"", ""LogLevel"", ""Category"", ""Message"", ""Exception"", ""UserName"", ""IpAddress"", ""RequestPath"", ""RequestMethod"", ""Properties"") 
                                   VALUES (@timestamp::timestamptz, @level, @logger, @message, @exception, @username, @ipaddress, @requestpath, @requestmethod, @properties)",
                    KeepConnection = false
                };
                
                // Add parameters
                databaseTarget.Parameters.Add(new NLog.Targets.DatabaseParameterInfo("@timestamp", "${date:universalTime=true:format=yyyy-MM-dd HH\\:mm\\:ss.fff zzz}"));
                // Convert short log levels to full names to match ActivityLogService format
                var levelLayout = "${replace:inner=${replace:inner=${replace:inner=${replace:inner=${level}:searchFor=Info:replaceWith=Information}:searchFor=Warn:replaceWith=Warning}:searchFor=Error:replaceWith=Error}:searchFor=Debug:replaceWith=Debug}";
                databaseTarget.Parameters.Add(new NLog.Targets.DatabaseParameterInfo("@level", levelLayout));
                databaseTarget.Parameters.Add(new NLog.Targets.DatabaseParameterInfo("@logger", "${logger}"));
                databaseTarget.Parameters.Add(new NLog.Targets.DatabaseParameterInfo("@message", "${message}"));
                databaseTarget.Parameters.Add(new NLog.Targets.DatabaseParameterInfo("@exception", "${exception:format=tostring}"));
                databaseTarget.Parameters.Add(new NLog.Targets.DatabaseParameterInfo("@username", "${aspnet-user-identity}"));
                databaseTarget.Parameters.Add(new NLog.Targets.DatabaseParameterInfo("@ipaddress", "${aspnet-request-ip}"));
                databaseTarget.Parameters.Add(new NLog.Targets.DatabaseParameterInfo("@requestpath", "${aspnet-request-url:IncludeHost=false:IncludePort=false:IncludeQueryString=false}"));
                databaseTarget.Parameters.Add(new NLog.Targets.DatabaseParameterInfo("@requestmethod", "${aspnet-request-method}"));
                databaseTarget.Parameters.Add(new NLog.Targets.DatabaseParameterInfo("@properties", "${all-event-properties}"));
                
                config.AddTarget(databaseTarget);
                
                // Add database logging rules (targeted and efficient)
                config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Ribosoft.*");
                config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, databaseTarget, "NCBI.Datasets.API.*");
                
                // Hangfire - specific rules only (no broad "Hangfire.*" to avoid duplicates)
                config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Hangfire.PostgreSql.*");
                config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Hangfire.SqlServer.*");
                config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Hangfire.Processing.*");
                config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Hangfire.Server.*");
                config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Hangfire.BackgroundJobServer");
                config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, databaseTarget, "Hangfire.Storage.*");
                
                // Microsoft framework logs - important events only
                config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, databaseTarget, "Microsoft.AspNetCore.Authentication.*");
                config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, databaseTarget, "Microsoft.AspNetCore.Authorization.*");
                config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, databaseTarget, "Microsoft.EntityFrameworkCore.*");
                
                // Capture errors from other Microsoft/System components
                config.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Fatal, databaseTarget, "Microsoft.*");
                config.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Fatal, databaseTarget, "System.*");
                
                // Apply the configuration
                NLog.LogManager.Configuration = config;
            }
            else
            {
                // Fall back to file-only configuration
                NLog.LogManager.Setup().LoadConfigurationFromFile("nlog.config");
            }
            
            // Configure services
            ConfigureServices(builder.Services, builder.Configuration);
            
            // Configure logging
            builder.Logging.ClearProviders();
            builder.Host.UseNLog();
            
            var app = builder.Build();
            
            // Initialize database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await ApplicationDbInitializer.Initialize(services);
            }
            
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

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
        }
        
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
    }
}
