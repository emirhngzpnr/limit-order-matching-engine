using MatchEngine.Persistence;

namespace MatchEngine.Tests;

public class OrderCommandProcessorTests
{

    [Fact]
    public async Task SubmitAsync_DoesNotApplyOrderWhenPersistenceFails()
    {
        var store = new FailingOrderCommandStore();
        var book = new OrderBook();

        var processor = new OrderCommandProcessor(store, book);

        var command = new SubmitOrderCommand(
            Guid.NewGuid(),
            5,
            100,
            OrderSide.Sell);

        // Kayıt hatası çağırana ulaşmalı.
        var exception = await Assert.ThrowsAsync<IOException>(
            () => processor.SubmitAsync(command));

        Assert.Equal("Test kayıt hatası.", exception.Message);

        // Kayıt başarısız olduğu için emir uygulanmamalı.
        Assert.Empty(book.GetSellOrders());
        Assert.Empty(book.GetBuyOrders());

        // Hata sonrası yeni komut kabul edilmemeli.
        var nextCommand = new SubmitOrderCommand(
            Guid.NewGuid(),
            2,
            100,
            OrderSide.Buy);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => processor.SubmitAsync(nextCommand));

        // İkinci komut kayıt bileşenine bile ulaşmamalı.
        Assert.Equal(1, store.AppendCallCount);

        Assert.Empty(book.GetSellOrders());
        Assert.Empty(book.GetBuyOrders());
    }

    private sealed class FailingOrderCommandStore : IOrderCommandStore
    {
        public int AppendCallCount { get; private set; }

        public Task AppendAsync(OrderCommand command)
        {
            AppendCallCount++;

            return Task.FromException(
                new IOException("Test kayıt hatası."));
        }

        public Task<IReadOnlyList<StoredOrderCommand>> ReadBatchAsync(
            long afterSequenceNumber,
            int batchSize)
        {
            throw new NotSupportedException(
                "Bu testte okuma işlemi kullanılmıyor.");
        }
    }
}