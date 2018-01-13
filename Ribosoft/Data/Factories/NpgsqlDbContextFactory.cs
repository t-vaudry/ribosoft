using Microsoft.EntityFrameworkCore;

namespace Ribosoft.Data.Factories
{
    public class NpgsqlDbContextFactory : DbContextFactory<NpgsqlDbContext>
    {
        protected override NpgsqlDbContext CreateNewInstance(DbContextOptionsBuilder<NpgsqlDbContext> options,
            string connectionString)
        {
            options.UseNpgsql(connectionString);

            return new NpgsqlDbContext(options.Options);
        }
    }
}
