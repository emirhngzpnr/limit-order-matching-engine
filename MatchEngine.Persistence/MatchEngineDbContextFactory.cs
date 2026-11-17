using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MatchEngine.Persistence;

public sealed class MatchEngineDbContextFactory : IDesignTimeDbContextFactory<MatchEngineDbContext>
{
    public MatchEngineDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("MATCHENGINE_SQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "MATCHENGINE_SQL_CONNECTION ortam değişkenini ayarla.");
        }

        var options = new DbContextOptionsBuilder<MatchEngineDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new MatchEngineDbContext(options);
    }
}
