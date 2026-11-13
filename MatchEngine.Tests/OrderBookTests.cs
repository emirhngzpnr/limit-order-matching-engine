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
            var trades = book.Submit(buyOrder);
            var trade = Assert.Single(trades);
            Assert.Equal(5, trade.Quantity);
            Assert.Equal(100, trade.Price);
        }
        [Fact]
        public void Submit_MatchesSellOrderWithWaitingBuyOrder()
        {
            var book = new OrderBook();
            var buyOrder = new Order(5, 100, OrderSide.Buy);
            var sellOrder = new Order(5, 100, OrderSide.Sell);

            book.Submit(buyOrder);
            var trades = book.Submit(sellOrder);

            var trade = Assert.Single(trades);
            Assert.Equal(5, trade.Quantity);
            Assert.Equal(100, trade.Price);
            Assert.Empty(book.GetBuyOrders());
            Assert.Empty(book.GetSellOrders());
        }

        [Fact]
        public void Submit_MatchesBuyOrderAtWaitingSellPrice()
        {
            var book = new OrderBook();
            var sellOrder = new Order(5, 100, OrderSide.Sell);
            var buyOrder = new Order(5, 101, OrderSide.Buy);
            book.Submit(sellOrder);
            var trades = book.Submit(buyOrder);
            var trade = Assert.Single(trades);
            Assert.Equal(5, trade.Quantity);
            Assert.Equal(100, trade.Price);
            Assert.Empty(book.GetBuyOrders());
            Assert.Empty(book.GetSellOrders());
        }
        [Fact]
        public void Submit_DoesNotMatchBuyOrderBelowWaitingSellPrice()
        {
            var book = new OrderBook();
            var sellOrder = new Order(5, 100, OrderSide.Sell);
            var buyOrder = new Order(5, 99, OrderSide.Buy);
            book.Submit(sellOrder);
            var trades = book.Submit(buyOrder);
            Assert.Empty(trades);

            var waitingBuy = Assert.Single(book.GetBuyOrders());
            Assert.Equal(99, waitingBuy.LimitPrice);

            var waitingSell = Assert.Single(book.GetSellOrders());
            Assert.Equal(100, waitingSell.LimitPrice);
        }

        [Fact]
        public void Submit_MatchesSellOrderAtWaitingBuyPrice()
        {
            var book = new OrderBook();
            var buyOrder = new Order(5, 101, OrderSide.Buy);
            var sellOrder = new Order(5, 100, OrderSide.Sell);
            book.Submit(buyOrder);
            var trades = book.Submit(sellOrder);
            var trade = Assert.Single(trades);
            Assert.Equal(101, trade.Price);
            Assert.Equal(5, trade.Quantity);
            Assert.Empty(book.GetBuyOrders());
            Assert.Empty(book.GetSellOrders());
        }

    }
}
