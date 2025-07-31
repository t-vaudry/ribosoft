using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ribosoft.Models;

namespace Ribosoft.Data
{
    public static class ApplicationDbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            using (var context = serviceProvider.GetRequiredService<ApplicationDbContext>())
            {
                try
                {
                    // Apply any pending migrations
                    logger.LogInformation("Checking for pending database migrations...");
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Found {Count} pending migrations: {Migrations}",
                            pendingMigrations.Count(),
                            string.Join(", ", pendingMigrations));

                        logger.LogInformation("Applying database migrations...");
                        await context.Database.MigrateAsync();
                        logger.LogInformation("Database migrations applied successfully");
                    }
                    else
                    {
                        logger.LogInformation("Database is up to date, no migrations needed");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while applying database migrations");
                    throw;
                }

                // Initialize roles and seed data
                await EnsureRole(serviceProvider, "Administrator");
                await EnsureRole(serviceProvider, "Guest");

                SeedDatabase(context);
            }
        }

        private static async Task<IdentityResult?> EnsureRole(IServiceProvider serviceProvider, string role)
        {
            IdentityResult? identityResult = null;
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (roleManager != null && !await roleManager.RoleExistsAsync(role))
            {
                identityResult = await roleManager.CreateAsync(new IdentityRole(role));
            }

            return identityResult;
        }

        public static void SeedDatabase(ApplicationDbContext context)
        {
            //
        }
    }
}
