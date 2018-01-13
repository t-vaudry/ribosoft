using Microsoft.EntityFrameworkCore;

namespace Ribosoft.Data.Factories
{
    public class ApplicationDbContextFactory : DbContextFactory<ApplicationDbContext>
    {
        protected override ApplicationDbContext CreateNewInstance(DbContextOptionsBuilder<ApplicationDbContext> options,
            string connectionString)
        {
            options.UseSqlServer(connectionString);

            return new ApplicationDbContext(options.Options);
        }
    }
}
