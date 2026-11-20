using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine
{
    public class OrderReplay
    {
        public static OrderBook Replay(IEnumerable<OrderCommand> commands)
        {
            var book = new OrderBook();

            Apply(book, commands);

            return book;
        }

        public static void Apply(
            OrderBook book,
            IEnumerable<OrderCommand> commands)
        {
            foreach (var command in commands)
            {
                switch (command)
                {
                    case SubmitOrderCommand submit:
                        var order = new Order(
                            submit.OrderId,
                            submit.Quantity,
                            submit.LimitPrice,
                            submit.Side);

                        book.Submit(order);
                        break;

                    case CancelOrderCommand cancel:
                        book.Cancel(cancel.OrderId);
                        break;

                    default:
                        throw new ArgumentException(
                            "Desteklenmeyen komut türü.",
                            nameof(commands));
                }
            }
        }
    }
}
