using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine
{
    public class Trade
    {
        int Price { get;}
        int Quantity { get;}
        

        public Trade(int price,int quantity)
        {
            Price = price;
            Quantity = quantity;
           
        }
    }
}
