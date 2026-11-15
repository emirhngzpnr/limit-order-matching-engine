using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace MatchEngine
{
    public class OrderBook
    {
        private readonly List<RestingOrder> _buyOrders = new List<RestingOrder>();
        private readonly List<RestingOrder> _sellOrders = new List<RestingOrder>();

        public IReadOnlyList<Trade> Submit(Order order)
        {
            #region     Alış İşlemleri
            if (order.Side == OrderSide.Buy)
            {
                var incomingBuy = new RestingOrder(order);
                var trades = new List<Trade>();

                while (incomingBuy.RemainingQuantity > 0)
                {
                    var sellOrder = _sellOrders
                        .Where(sell => sell.Order.LimitPrice <= order.LimitPrice)
                        .OrderBy(sell => sell.Order.LimitPrice)
                        .FirstOrDefault();

                    if (sellOrder == null)
                    {
                        break;
                    }

                    int filledQuantity = Math.Min(
                        incomingBuy.RemainingQuantity,
                        sellOrder.RemainingQuantity);

                    incomingBuy.Fill(filledQuantity);
                    sellOrder.Fill(filledQuantity);

                    trades.Add(new Trade(sellOrder.Order.LimitPrice, filledQuantity));

                    if (sellOrder.RemainingQuantity == 0)
                    {
                        _sellOrders.Remove(sellOrder);
                    }
                }

                if (incomingBuy.RemainingQuantity > 0)
                {
                    _buyOrders.Add(incomingBuy);
                }

                return trades;
            }
            #endregion

            #region  Satış İşlemleri
            else if (order.Side == OrderSide.Sell)
            {
                var incomingSell = new RestingOrder(order);
                var trades = new List<Trade>();

                while (incomingSell.RemainingQuantity > 0)
                {
                    var buyOrder = _buyOrders.Where(
                  buy =>
                  buy.Order.LimitPrice >= order.LimitPrice
                  )
                  .OrderByDescending(buy => buy.Order.LimitPrice)
                  .FirstOrDefault();

                    if (buyOrder == null)
                    {
                        break;
                    }
                    int filledQuantity = Math.Min(
                        incomingSell.RemainingQuantity,
                        buyOrder.RemainingQuantity);

                    incomingSell.Fill(filledQuantity);
                    buyOrder.Fill(filledQuantity);

                    trades.Add(new Trade(buyOrder.Order.LimitPrice, filledQuantity));
                    
                    if (buyOrder.RemainingQuantity == 0)
                    {
                        _buyOrders.Remove(buyOrder);
                    }
                }

              
                if (incomingSell.RemainingQuantity > 0)
                {
                    _sellOrders.Add(incomingSell);
                }
                return trades;

                }
            #endregion

            return Array.Empty<Trade>();
        }

        public bool Cancel(Guid orderId)
        {
            var waitingBuy = _buyOrders
                .Find(restingOrder => restingOrder.Order.Id==orderId);
            
            if (waitingBuy != null)
            {
                _buyOrders.Remove(waitingBuy);
                return true;
            }
            var waitingSell = _sellOrders
                .Find(restingOrder => restingOrder.Order.Id == orderId);
           
            if (waitingSell != null)
            {
                _sellOrders.Remove(waitingSell);
                return true;
            }

            return false;
        }
        public OrderSnapshot[] GetBuyOrders()
        {
            return _buyOrders
                   .Select(buy => new OrderSnapshot(buy.Order, buy.RemainingQuantity))
                   .ToArray();
        }
        public OrderSnapshot[] GetSellOrders()
        {
            return _sellOrders
                    .Select(sell => new OrderSnapshot(sell.Order, sell.RemainingQuantity))
                    .ToArray();
        }


    }
}
