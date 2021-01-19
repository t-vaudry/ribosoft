using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Ribosoft.Data;
using Ribosoft.Models;
using Ribosoft.Services;
using Hangfire.Logging.LogProviders;

namespace Ribosoft
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var providerName = Configuration.GetValue("EntityFrameworkProvider", "SqlServer");
            var connectionString = Configuration.GetConnectionString($"{providerName}Connection");

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
                .UsePostgreSqlStorage(connectionString, new PostgreSqlStorageOptions
                {
                    InvisibilityTimeout = TimeSpan.FromDays(1)
                }));
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

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services
                .AddMvc(config =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    config.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

            services.AddCloudscribePagination();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                BrowserLinkExtensions.UseBrowserLink(app);
                DatabaseErrorPageExtensions.UseDatabaseErrorPage(app);
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
