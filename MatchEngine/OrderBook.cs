using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine
{
    public class OrderBook
    {
        private readonly List<Order> _byOrders = new List<Order>();
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
                _byOrders.Add(order);

            }
            if (order.Side == OrderSide.Sell)
            {
                _sellOrders.Add(order);
            }

            return Array.Empty<Trade>();
        }
        public Order[] GetBuyOrders()
        {
            return _byOrders.ToArray();
        }
        public Order[] GetSellOrders()
        {
            return _sellOrders.ToArray();
        }


    }
}
