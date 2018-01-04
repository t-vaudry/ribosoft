using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ribosoft.Models;

namespace Ribosoft.Data
{
    public static class ApplicationDbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context =
                new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                context.Database.EnsureCreated();

                await EnsureRole(serviceProvider, "Administrator");

                SeedDatabase(context);
            }
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider, string role)
        {
            IdentityResult identityResult = null;
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(role))
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
