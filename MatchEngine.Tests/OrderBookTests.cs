using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Fact]
        public void Submit_AddsSellOrderToEmptyBook()
        {
            var book = new OrderBook();
            var order = new Order(3, 105, OrderSide.Sell);
            book.Submit(order);
            var waitingOrder = Assert.Single(book.GetSellOrders());
            Assert.Equal(3, waitingOrder.Quantity);
            Assert.Equal(105, waitingOrder.LimitPrice);
            Assert.Empty(book.GetBuyOrders());

        }

        [Fact]
        public void Submit_MatchesBuyOrderWithWaitingSellOrder()
        {
            var book = new OrderBook();
            var sellOrder = new Order(5, 100, OrderSide.Sell);
            var buyOrder = new Order(5, 100, OrderSide.Buy);
            book.Submit(sellOrder);
      

        }


    }
}
