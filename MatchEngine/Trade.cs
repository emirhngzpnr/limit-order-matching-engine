using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine
{
    public class Trade
    {
        public Guid BuyOrderId { get; }
        public Guid SellOrderId { get; }
        public int Price { get;}
        public int Quantity { get;}
        
        
        public Trade(int price,int quantity,Guid sellOrderId,Guid buyOrderId)
        {
            Price = price;
            Quantity = quantity;
            BuyOrderId = buyOrderId;
            SellOrderId = sellOrderId;
        }
    }
}
