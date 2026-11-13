using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine
{
    public class Trade
    {
        public int Price { get;}
        public int Quantity { get;}
        

        public Trade(int price,int quantity)
        {
            Price = price;
            Quantity = quantity;
           
        }
    }
}
