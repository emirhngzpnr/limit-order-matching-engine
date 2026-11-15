using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Xunit.Sdk;

namespace MatchEngine.Tests
{
    public class OrderBookTests
    {
        [Fact]
        public void Submit_AddsBuyOrderToEmptyBook()
        {
            var book = new OrderBook();
            var order = new Order(5, 100, OrderSide.Buy);
            book.Submit(order);
            var waitingOrder = Assert.Single(book.GetBuyOrders());
            Assert.Equal(5, waitingOrder.Order.Quantity);
            Assert.Equal(100, waitingOrder.Order.LimitPrice);
        }
        [Fact]
        public void Submit_AddsSellOrderToEmptyBook()
        {
            var book = new OrderBook();
            var order = new Order(3, 105, OrderSide.Sell);
            book.Submit(order);
            var waitingOrder = Assert.Single(book.GetSellOrders());
            Assert.Equal(3, waitingOrder.Order.Quantity);
            Assert.Equal(105, waitingOrder.Order.LimitPrice);
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
            Assert.Equal(99, waitingBuy.Order.LimitPrice);

            var waitingSell = Assert.Single(book.GetSellOrders());
            Assert.Equal(100, waitingSell.Order.LimitPrice);
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
        [Fact]
        public void Submit_MatchesBuyOrderWithLowestWaitingSellPrice()
        {
            var book = new OrderBook();
            var firstSellOrder = new Order(5, 101, OrderSide.Sell);
            var secondSellOrder = new Order(5, 100, OrderSide.Sell);
            var buyOrder = new Order(5, 101, OrderSide.Buy);
            book.Submit(firstSellOrder);
            book.Submit(secondSellOrder);
            var trades = book.Submit(buyOrder);
            var trade = Assert.Single(trades);
            Assert.Equal(100, trade.Price);
            Assert.Equal(5, trade.Quantity);
            var waitingSell = Assert.Single(book.GetSellOrders());
            Assert.Equal(101, waitingSell.Order.LimitPrice);
            Assert.Empty(book.GetBuyOrders());

        }
        [Fact]
        public void Submit_MatchesSellOrderWithHighestWaitingBuyPrice()
        {
            var book = new OrderBook();
            var firstBuyOrder = new Order(5, 100, OrderSide.Buy);
            var secondBuyOrder = new Order(5, 101, OrderSide.Buy);
            var sellOrder = new Order(5, 100, OrderSide.Sell);
            book.Submit(firstBuyOrder);
            book.Submit(secondBuyOrder);
            var trades = book.Submit(sellOrder);
            var trade = Assert.Single(trades);
            Assert.Equal(101, trade.Price);
            Assert.Equal(5, trade.Quantity);
            var waitingBuy = Assert.Single(book.GetBuyOrders());
            Assert.Equal(100, waitingBuy.Order.LimitPrice);
            Assert.Empty(book.GetSellOrders());

        }
        [Fact]
        public void Submit_MatchesOldestSellOrderWhenPricesAreEqual()
        {
            var book = new OrderBook();
            var firstSellOrder = new Order(5, 100, OrderSide.Sell);
            var secondSellOrder = new Order(5, 100, OrderSide.Sell);
            var buyOrder = new Order(5, 100, OrderSide.Buy);
            book.Submit(firstSellOrder);
            book.Submit(secondSellOrder);
            var trades = book.Submit(buyOrder);
            var trade = Assert.Single(trades);
            Assert.Equal(100, trade.Price);
            Assert.Equal(5, trade.Quantity);
            var waitingSell = Assert.Single(book.GetSellOrders());
            Assert.Same(secondSellOrder, waitingSell.Order);
            Assert.Empty(book.GetBuyOrders());

        }
        [Fact]
        public void Submit_MatchesOldestBuyOrderWhenPricesAreEqual()
        {
            var book = new OrderBook();
            var firstBuyOrder = new Order(5, 100, OrderSide.Buy);
            var secondBuyOrder = new Order(5, 100, OrderSide.Buy);
            var sellOrder = new Order(5, 100, OrderSide.Sell);
            book.Submit(firstBuyOrder);
            book.Submit(secondBuyOrder);
            var trades = book.Submit(sellOrder);
            var trade = Assert.Single(trades);
            Assert.Equal(100, trade.Price);
            Assert.Equal(5, trade.Quantity);
            var waitingBuy = Assert.Single(book.GetBuyOrders());
            Assert.Same(secondBuyOrder, waitingBuy.Order);
            Assert.Empty(book.GetSellOrders());

        }

        [Fact]
        public void Submit_PartiallyFillsWaitingSellOrder()
        {
            var book = new OrderBook();
            var sellOrder = new Order(5, 100, OrderSide.Sell);
            var buyOrder = new Order(2, 100, OrderSide.Buy);
            book.Submit(sellOrder);
            var trades = book.Submit(buyOrder);
            var trade = Assert.Single(trades);
            Assert.Equal(100, trade.Price);
            Assert.Equal(2, trade.Quantity);
            var waitingSell = Assert.Single(book.GetSellOrders());
            Assert.Equal(3, waitingSell.RemainingQuantity);
            Assert.Empty(book.GetBuyOrders());
        }

        [Fact]
        public void Submit_AddsRemainingBuyQuantityToBook()
        {
            var book = new OrderBook();
            var sellOrder = new Order(2, 100, OrderSide.Sell);
            var buyOrder = new Order(5, 100, OrderSide.Buy);
            book.Submit(sellOrder);
            var trades = book.Submit(buyOrder);
            var trade = Assert.Single(trades);
            Assert.Equal(100, trade.Price);
            Assert.Equal(2, trade.Quantity);
            var waitingBuy = Assert.Single(book.GetBuyOrders());
            Assert.Equal(3, waitingBuy.RemainingQuantity);
            Assert.Empty(book.GetSellOrders());
        }
        [Fact]
        public void Submit_PartiallyFillsWaitingBuyOrder()
        {
            var book = new OrderBook();
            var buyOrder = new Order(5, 100, OrderSide.Buy);
            var sellOrder = new Order(2, 100, OrderSide.Sell);
            book.Submit(buyOrder);
            var trades = book.Submit(sellOrder);
            var trade = Assert.Single(trades);
            Assert.Equal(100, trade.Price);
            Assert.Equal(2, trade.Quantity);
            var waitingBuy = Assert.Single(book.GetBuyOrders());
            Assert.Equal(3, waitingBuy.RemainingQuantity);
            Assert.Empty(book.GetSellOrders());
        }

        [Fact]
        public void Submit_AddsRemainingSellQuantityToBook()
        {
            var book = new OrderBook();
            var buyOrder = new Order(2, 100, OrderSide.Buy);
            var sellOrder = new Order(5, 100, OrderSide.Sell);
            book.Submit(buyOrder);
            var trades = book.Submit(sellOrder);
            var trade = Assert.Single(trades);
            Assert.Equal(100, trade.Price);
            Assert.Equal(2, trade.Quantity);
            var waitingSell = Assert.Single(book.GetSellOrders());
            Assert.Equal(3, waitingSell.RemainingQuantity);
            Assert.Empty(book.GetBuyOrders());
        }

        [Fact]
        public void Submit_MatchesBuyOrderWithMultipleWaitingSellOrders()
        {
            var book = new OrderBook();
            var firstSellorder = new Order(2, 100, OrderSide.Sell);
            var secondSellorder = new Order(4, 101, OrderSide.Sell);
            var buyOrder = new Order(5, 101, OrderSide.Buy);
            book.Submit(firstSellorder);
            book.Submit(secondSellorder);
            var trades = book.Submit(buyOrder);
            Assert.Collection(trades,
         first =>
       {
           Assert.Equal(100, first.Price);
           Assert.Equal(2, first.Quantity);
       },
         second =>
       {
           Assert.Equal(101, second.Price);
           Assert.Equal(3, second.Quantity);
       });

            var waitingSell = Assert.Single(book.GetSellOrders());
            Assert.Equal(101, waitingSell.Order.LimitPrice);
            Assert.Equal(1, waitingSell.RemainingQuantity);
            Assert.Empty(book.GetBuyOrders());
        }

        [Fact]
        public void Submit_MatchesSellOrderWithMultipleWaitingBuyOrders()
        {
            var book = new OrderBook();
            var firstBuyOrder = new Order(4, 100, OrderSide.Buy);
            var secondBuyOrder = new Order(2, 101, OrderSide.Buy);
            var sellOrder = new Order(5, 100, OrderSide.Sell);
            book.Submit(firstBuyOrder);
            book.Submit(secondBuyOrder);
            var trades = book.Submit(sellOrder);
            Assert.Collection(trades,
                first =>
                {
                    Assert.Equal(101, first.Price);
                    Assert.Equal(2, first.Quantity);
                },
                second =>
                {
                    Assert.Equal(100, second.Price);
                    Assert.Equal(3, second.Quantity);
                }
                );

            var waitingBuy = Assert.Single(book.GetBuyOrders());
            Assert.Equal(1, waitingBuy.RemainingQuantity);
            Assert.Empty(book.GetSellOrders());
        }

        [Fact]
        public void Cancel_RemovesOnlyOrderWithMatchingId()
        {
            var book = new OrderBook();
            var firstSellOrder = new Order(5, 100, OrderSide.Sell);
            var secondSellOrder = new Order(5, 100, OrderSide.Sell);
            book.Submit(firstSellOrder);
            book.Submit(secondSellOrder);
            Assert.True(book.Cancel(firstSellOrder.Id));
            var waitingSellOrder = Assert.Single(book.GetSellOrders());
            Assert.Equal(secondSellOrder.Id, waitingSellOrder.Order.Id);

        }

        [Fact]
        public void Cancel_ReturnsFalseWhenOrderIsNotWaiting()
        {
            var book = new OrderBook();
            var sellOrder = new Order(5, 100, OrderSide.Sell);
            book.Submit(sellOrder);
            Assert.False(book.Cancel(Guid.NewGuid()));
            var waitingSellOrder = Assert.Single(book.GetSellOrders());
            Assert.Equal(sellOrder.Id, waitingSellOrder.Order.Id);
        }
    }
}
