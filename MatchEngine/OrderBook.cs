using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace MatchEngine
{
    public class OrderBook
    {
        private readonly List<Order> _buyOrders = new List<Order>();
        private readonly List<Order> _sellOrders = new List<Order>();

        public IReadOnlyList<Trade> Submit(Order order)
        {
            if (order.Side == OrderSide.Buy)
            {
                var sellOrder = _sellOrders.Find(
                    sell =>
                    sell.LimitPrice == order.LimitPrice &&
                    sell.Quantity == order.Quantity
                    );
                if (sellOrder != null)
                {
                    _sellOrders.Remove(sellOrder);
                    return new[]
                    {
                        new Trade(
                            sellOrder.LimitPrice,
                            sellOrder.Quantity
                            )
                    };

                }
                _buyOrders.Add(order);

            }
            else if (order.Side == OrderSide.Sell)
            {
                var buyOrder = _buyOrders.Find(
                    buy =>
                    buy.LimitPrice == order.LimitPrice &&
                    buy.Quantity == order.Quantity
                    );
                if (buyOrder != null)
                {
                    _buyOrders.Remove(buyOrder);
                    return new[]
                    {
                        new Trade(
                            buyOrder.LimitPrice,
                            buyOrder.Quantity)
                    };

                }

                _sellOrders.Add(order);

            }

            return Array.Empty<Trade>();
        }
        public Order[] GetBuyOrders()
        {
            return _buyOrders.ToArray();
        }
        public Order[] GetSellOrders()
        {
            return _sellOrders.ToArray();
        }


    }
}
