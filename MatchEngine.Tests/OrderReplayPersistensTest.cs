using MatchEngine.Persistence.Mapping;
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

            await context.Database.OpenConnectionAsync();
        }

        [Fact]
        public async Task Replay_RestoresOrderBookFromSqlCommands()
        {
            var connectionString =
                Environment.GetEnvironmentVariable("MATCHENGINE_SQL_CONNECTION")
                ?? throw new InvalidOperationException(
                    "MATCHENGINE_SQL_CONNECTION ortam değişkenini ayarla.");

            var options = new DbContextOptionsBuilder<MatchEngineDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            var sellId = Guid.NewGuid();
            var buyId = Guid.NewGuid();

            var sellCommand =
                new SubmitOrderCommand(sellId, 5, 100, OrderSide.Sell);

            var buyCommand =
                new SubmitOrderCommand(buyId, 2, 100, OrderSide.Buy);

            try
            {
                // Komutları oluşma sırasıyla SQL'e kaydet.
                await using (var writeContext = new MatchEngineDbContext(options))
                {
                    writeContext.OrderCommands.Add(
                        OrderCommandMapper.FromSubmit(sellCommand));

                    await writeContext.SaveChangesAsync();

                    writeContext.OrderCommands.Add(
                        OrderCommandMapper.FromSubmit(buyCommand));

                    await writeContext.SaveChangesAsync();
                }

                // Yeni context ile veritabanından tekrar oku.
                await using var readContext = new MatchEngineDbContext(options);

                var entities = await readContext.OrderCommands
                    .AsNoTracking()
                    .Where(entity =>
                        entity.OrderId == sellId || entity.OrderId == buyId)
                    .OrderBy(entity => entity.SequenceNumber)
                    .ToListAsync();

                Assert.Equal(2, entities.Count);

                var commands = entities
                    .Select(entity => OrderCommandMapper.ToCommand(entity))
                    .ToArray();

                var restoredBook = OrderReplay.Replay(commands);

                var waitingSell = Assert.Single(restoredBook.GetSellOrders());

                Assert.Equal(sellId, waitingSell.Order.Id);
                Assert.Equal(3, waitingSell.RemainingQuantity);
                Assert.Empty(restoredBook.GetBuyOrders());
            }
            finally
            {
                // Test başarılı olsa da hata verse de kendi kayıtlarını temizle.
                await using var cleanupContext =
                    new MatchEngineDbContext(options);

                await cleanupContext.OrderCommands
                    .Where(entity =>
                        entity.OrderId == sellId || entity.OrderId == buyId)
                    .ExecuteDeleteAsync();
            }
        }
    }
}
