namespace MatchEngine.Persistence;

public class OrderCommandProcessor
{    private readonly OrderBook _book;
    private readonly IOrderCommandStore _store;
    private bool _requiresRecovery;

    public OrderCommandProcessor(
        IOrderCommandStore store,
        OrderBook book)
    {
        _store = store;
        _book = book;
    }

    public async Task<IReadOnlyList<Trade>> SubmitAsync(
        SubmitOrderCommand command)
    {
        if (_requiresRecovery)
        {
            throw new InvalidOperationException(
              "İşlemeye devam etmeden önce motor geri yüklenmelidir.");
        }

        ArgumentNullException.ThrowIfNull(command);

        // Geçersiz emirleri SQL'e yazmadan önce reddet.
        var order = new Order(
            command.OrderId,
            command.Quantity,
            command.LimitPrice,
            command.Side);

        if (command.Side != OrderSide.Buy &&
            command.Side != OrderSide.Sell)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command.Side));
        }

        _requiresRecovery = true;

        await _store.AppendAsync(command);

        var trades = _book.Submit(order);

        _requiresRecovery = false;

        return trades;
    }
}