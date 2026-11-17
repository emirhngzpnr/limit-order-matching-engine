using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine.Persistence.Mapping
{
    public static class OrderCommandMapper
    {
        public static OrderCommandEntity FromSubmit(
                SubmitOrderCommand command)
        {
            return new OrderCommandEntity(
               commandType: OrderCommandType.Submit,
               orderId: command.OrderId,
               quantity:command.Quantity,
               limitPrice: command.LimitPrice,
               side: command.Side);   
        }

        public static OrderCommandEntity FromCancel(
            CancelOrderCommand command)
        {
            return new OrderCommandEntity(
                OrderCommandType.Cancel,
                 command.OrderId,
                 quantity: null,
                 limitPrice: null,
                 side: null);
        }

        public static OrderCommand ToCommand(OrderCommandEntity entity)
        {
            switch (entity.CommandType)
            {
                case OrderCommandType.Submit:
                    if(entity.Quantity is null ||
                        entity.LimitPrice is null ||
                        entity.Side is null)
                    {
                        throw new InvalidOperationException(
                            "Submit kaydında adet, fiyat ve taraf bulunmalıdır)");
                    }
                    return new SubmitOrderCommand(
                                  entity.OrderId,
                                  entity.Quantity.Value,
                                  entity.LimitPrice.Value,
                                  entity.Side.Value);
                case OrderCommandType.Cancel:
                    return new CancelOrderCommand(entity.OrderId);
                default:
                    throw new InvalidOperationException(
                        $"Bilinmeyen Komut türü: {entity.CommandType}");
            }
        }
    }
}
