using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace MatchEngine
{
    public class OrderBook
    {
        private readonly List<RestingOrder> _buyOrders = new List<RestingOrder>();
        private readonly List<RestingOrder> _sellOrders = new List<RestingOrder>();

        public IReadOnlyList<Trade> Submit(Order order)
        {
            if (order.Side == OrderSide.Buy)
            {
                var sellOrder = _sellOrders.Where(
                    sell =>
                    sell.Order.LimitPrice <= order.LimitPrice
                    )
                    .OrderBy(sell => sell.Order.LimitPrice)
                    .FirstOrDefault();
                if (sellOrder != null)
                {
                    int filledQuantity = Math.Min(order.Quantity, sellOrder.RemainingQuantity);
                    sellOrder.Fill(filledQuantity);
                    if (sellOrder.RemainingQuantity == 0)
                    {
                        _sellOrders.Remove(sellOrder);
                    }
                    int remainingBuyQuantity = order.Quantity - filledQuantity;

                    if (remainingBuyQuantity > 0)
                    {
                        var waitingBuy = new RestingOrder(order);
                        waitingBuy.Fill(filledQuantity);
                        _buyOrders.Add(waitingBuy);
                    }
                    return new[]
                    {
                        new Trade(
                            sellOrder.Order.LimitPrice,
                            filledQuantity
                            )
                    };

                }
                _buyOrders.Add(new RestingOrder(order));

            }
            else if (order.Side == OrderSide.Sell)
            {
                var buyOrder = _buyOrders.Where(
                    buy =>
                    buy.Order.LimitPrice >= order.LimitPrice &&
                    buy.RemainingQuantity == order.Quantity
                    )
                    .OrderByDescending(buy => buy.Order.LimitPrice)
                    .FirstOrDefault();
                if (buyOrder != null)
                {
                    _buyOrders.Remove(buyOrder);
                    return new[]
                    {
                        new Trade(
                            buyOrder.Order.LimitPrice,
                            buyOrder.RemainingQuantity)
                    };

                }

                _sellOrders.Add(new RestingOrder(order));

            }

            return Array.Empty<Trade>();
        }
        public OrderSnapshot[] GetBuyOrders()
        {
            return _buyOrders
                   .Select(buy => new OrderSnapshot(buy.Order, buy.RemainingQuantity))
                   .ToArray();
        }
        public OrderSnapshot[] GetSellOrders()
        {
            return _sellOrders
                    .Select(sell => new OrderSnapshot(sell.Order, sell.RemainingQuantity))
                    .ToArray();
        }


    }
}
