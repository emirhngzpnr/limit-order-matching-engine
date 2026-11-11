using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine.Tests
{
    public class OrderBookTests
    {
        [Fact]
        public void Submit_AddsBuyOrderToEmptyBook()
        {
            var book = new OrderBook();
            var order = new Order(5,100,OrderSide.Buy);
            book.Submit(order);
            var waitingOrder = Assert.Single(book.GetBuyOrders());
            Assert.Equal(5, waitingOrder.Quantity);
            Assert.Equal(100, waitingOrder.LimitPrice);
        }
    }
}
