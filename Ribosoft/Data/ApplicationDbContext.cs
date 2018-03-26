using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ribosoft.Models;

namespace Ribosoft.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            TouchTimestamps();

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            TouchTimestamps();

            return base.SaveChangesAsync(cancellationToken);
        }

        public void TouchTimestamps()
        {
            // get entries that are being Added or Updated
            var modifiedEntries = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);

            var now = DateTime.UtcNow;

            foreach (var entry in modifiedEntries)
            {
                if (!(entry.Entity is BaseEntity entity))
                {
                    continue;
                }

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = now;
                }
                else
                {
                    entry.Property("CreatedAt").IsModified = false;
                }

                entity.UpdatedAt = now;
            }
        }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<Ribozyme> Ribozymes { get; set; }
        public DbSet<RibozymeStructure> RibozymeStructures { get; set; }
        public DbSet<Design> Designs { get; set; }
        public DbSet<Assembly> Assemblies { get; set; }
    }
}
