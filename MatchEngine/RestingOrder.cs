using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine
{
    internal class RestingOrder
    {
        public Order Order { get; }
        public int RemainingQuantity { get; private set; }
        public RestingOrder(Order order)
        {
            Order = order;
            RemainingQuantity = order.Quantity;
        }
        public void Fill(int quantity)
        {
            if(quantity<=0 || quantity > RemainingQuantity)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity));
            }

            RemainingQuantity -= quantity;
        }
    }
}
