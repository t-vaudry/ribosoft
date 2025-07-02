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
using Hangfire.Logging.LogProviders;
using System.Diagnostics.CodeAnalysis;

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
                .UseLogProvider(new ColouredConsoleLogProvider())
                .UsePostgreSqlStorage(connectionString));
        }
        else if (providerName == "SqlServer")
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddHangfire(x => x
                .UseLogProvider(new ColouredConsoleLogProvider())
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
        services.AddTransient<IEmailSender, EmailSender>();

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
        services.AddHangfireServer();
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
