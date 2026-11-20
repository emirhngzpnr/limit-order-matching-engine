using MatchEngine.Persistence;
using MatchEngine.Persistence.Mapping;
using Microsoft.Data.SqlClient;
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

        [Fact]
        public async Task ReadBatchAsync_ReadsCommandsInOrderWithoutRepeating()
        {
            var connectionString =
                Environment.GetEnvironmentVariable("MATCHENGINE_SQL_CONNECTION")
                ?? throw new InvalidOperationException(
                    "MATCHENGINE_SQL_CONNECTION ortam değişkenini ayarla.");

            var options = new DbContextOptionsBuilder<MatchEngineDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            await using var context = new MatchEngineDbContext(options);

            // Test kayıtları kalıcı olarak bırakılmayacak.
            await using var transaction =
                await context.Database.BeginTransactionAsync();

            var store = new SqlOrderCommandStore(context);

            // Daha önceki kayıtları bu testin dışında tut.
            var startingSequence = await context.OrderCommands
                .MaxAsync(entity => (long?)entity.SequenceNumber) ?? 0;

            var sellId = Guid.NewGuid();
            var buyId = Guid.NewGuid();

            var sellCommand =
                new SubmitOrderCommand(sellId, 5, 100, OrderSide.Sell);

            var buyCommand =
                new SubmitOrderCommand(buyId, 2, 100, OrderSide.Buy);

            var cancelCommand = new CancelOrderCommand(sellId);

            await store.AppendAsync(sellCommand);
            await store.AppendAsync(buyCommand);
            await store.AppendAsync(cancelCommand);

            var firstBatch = await store.ReadBatchAsync(
                startingSequence, 2);

            Assert.Equal(2, firstBatch.Count);
            Assert.Equal<OrderCommand>(sellCommand, firstBatch[0].Command);
            Assert.Equal<OrderCommand>(buyCommand, firstBatch[1].Command);

            Assert.True(
                firstBatch[0].SequenceNumber < firstBatch[1].SequenceNumber);

            var secondBatch = await store.ReadBatchAsync(
                firstBatch[1].SequenceNumber, 2);

            var remainingCommand = Assert.Single(secondBatch);

            Assert.Equal<OrderCommand>(
                cancelCommand, remainingCommand.Command);

            Assert.True(
                remainingCommand.SequenceNumber > firstBatch[1].SequenceNumber);

            var lastBatch = await store.ReadBatchAsync(
                remainingCommand.SequenceNumber, 2);

            Assert.Empty(lastBatch);

            await transaction.RollbackAsync();
        }

        [Fact]
        public void Apply_PreservesOrderBookStateBetweenBatches()
        {
            var book = new OrderBook();
            var sellId = Guid.NewGuid();
            var buyId = Guid.NewGuid();

            OrderCommand[] firstBatch =
            {
                new SubmitOrderCommand(sellId, 5, 100, OrderSide.Sell)
            };

            OrderCommand[] secondBatch =
            {
                 new SubmitOrderCommand(buyId, 2, 100, OrderSide.Buy)
            };

            OrderReplay.Apply(book, firstBatch);
            OrderReplay.Apply(book, secondBatch);

            var waitingSell = Assert.Single(book.GetSellOrders());

            Assert.Equal(sellId, waitingSell.Order.Id);
            Assert.Equal(3, waitingSell.RemainingQuantity);
            Assert.Empty(book.GetBuyOrders());
        }

        [Fact]
        public async Task RestoreAsync_RestoresOrderBookAcrossMultipleBatches()
        {
            var connectionString =
                Environment.GetEnvironmentVariable("MATCHENGINE_SQL_CONNECTION")
                ?? throw new InvalidOperationException(
                    "MATCHENGINE_SQL_CONNECTION ortam değişkenini ayarla.");

            // Mevcut bağlantıyı kullan, veritabanı adını teste özel değiştir.
            var connectionBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = $"MatchEngineTest_Recovery_{Guid.NewGuid():N}"
            };

            var options = new DbContextOptionsBuilder<MatchEngineDbContext>()
                .UseSqlServer(connectionBuilder.ConnectionString)
                .Options;

            var sellId = Guid.NewGuid();
            var buyId = Guid.NewGuid();

            try
            {
                await using (var writeContext = new MatchEngineDbContext(options))
                {
                    // Test veritabanını ve tablolarını migration ile oluştur.
                    await writeContext.Database.MigrateAsync();

                    var writeStore = new SqlOrderCommandStore(writeContext);

                    await writeStore.AppendAsync(
                        new SubmitOrderCommand(
                            sellId, 5, 100, OrderSide.Sell));

                    await writeStore.AppendAsync(
                        new SubmitOrderCommand(
                            buyId, 2, 100, OrderSide.Buy));
                }

                // Kayıtları yeni bir context üzerinden geri yükle.
                await using var readContext = new MatchEngineDbContext(options);

                var readStore = new SqlOrderCommandStore(readContext);
                var recovery = new OrderBookRecovery(readStore);

                var restoredBook = await recovery.RestoreAsync(batchSize: 1);

                var waitingSell = Assert.Single(restoredBook.GetSellOrders());

                Assert.Equal(sellId, waitingSell.Order.Id);
                Assert.Equal(100, waitingSell.Order.LimitPrice);
                Assert.Equal(3, waitingSell.RemainingQuantity);
                Assert.Empty(restoredBook.GetBuyOrders());
            }
            finally
            {
                // Sadece bu test için oluşturulan veritabanını sil.
                await using var cleanupContext =
                    new MatchEngineDbContext(options);

                await cleanupContext.Database.EnsureDeletedAsync();
            }
        }
    }
}
