using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine
{
    public abstract record OrderCommand;
    public sealed record SubmitOrderCommand(
        Guid OrderId,
        int Quantity,
        int LimitPrice,
        OrderSide Side
        ) : OrderCommand;

    public sealed record CancelOrderCommand(Guid OrderId) : OrderCommand;


}
