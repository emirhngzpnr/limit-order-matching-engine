using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine
{
    public class OrderBook
    {
        private readonly List<Order> _byOrders = new List<Order>();

       public void Submit(Order order)
        {
            if (order.Side == OrderSide.Buy)
            {
                _byOrders.Add(order);
            }
        }
        public Order[] GetBuyOrders()
        {
            return _byOrders.ToArray();
        }
    }
}
