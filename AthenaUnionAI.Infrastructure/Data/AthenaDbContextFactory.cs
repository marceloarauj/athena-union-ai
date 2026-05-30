using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AthenaUnionAI.Infrastructure.Data
{
    public class AthenaDbContextFactory : IDesignTimeDbContextFactory<AthenaDbContext>
    {
        public AthenaDbContext CreateDbContext(string[] args)
        {
            var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "../AthenaUnionAI");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(apiProjectPath, "appsettings.json"), optional: false, reloadOnChange: true)
                .AddJsonFile(Path.Combine(apiProjectPath, "appsettings.Development.json"), optional: true, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<AthenaDbContext>();
            optionsBuilder.UseNpgsql(connectionString, o => o.UseVector());

            return new AthenaDbContext(optionsBuilder.Options);
        }
    }
}
