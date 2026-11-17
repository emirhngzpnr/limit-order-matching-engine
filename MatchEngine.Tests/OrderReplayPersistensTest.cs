using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine.Tests
{
    public class OrderReplayPersistensTest
    {
        [Fact]
        public async Task Database_CanConnectToSqlServer()
        {
            var connectionString =
                Environment.GetEnvironmentVariable("MATCHENGINE_SQL_CONNECTION")
                ?? throw new InvalidOperationException(
                    "MATCHENGINE_SQL_CONNECTION ortam değişkenini ayarla.");

            var options = new DbContextOptionsBuilder<MatchEngineDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            await using var context = new MatchEngineDbContext(options);

            //Assert.True(await context.Database.CanConnectAsync());
            await context.Database.OpenConnectionAsync();
        }
    }
}
