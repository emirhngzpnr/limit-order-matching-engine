namespace MatchEngine.Persistence;

public class OrderBookRecovery
{
    private readonly SqlOrderCommandStore _store;

    public OrderBookRecovery(SqlOrderCommandStore store)
    {
        _store = store;
    }

    public async Task<OrderBook> RestoreAsync(int batchSize = 1000)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize));
        }

        var book = new OrderBook();
        long lastSequenceNumber = 0;

        while (true)
        {
            var batch = await _store.ReadBatchAsync(
                lastSequenceNumber,
                batchSize);

            if (batch.Count == 0)
            {
                break;
            }

            OrderReplay.Apply(
                book,
                batch.Select(item => item.Command));

            lastSequenceNumber =
                batch[batch.Count - 1].SequenceNumber;
        }

        return book;
    }
}