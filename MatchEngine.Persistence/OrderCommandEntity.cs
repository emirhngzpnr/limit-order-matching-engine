using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine.Persistence
{
    public class OrderCommandEntity
    {
        public long SequenceNumber { get; private set; }
        public OrderCommandType CommandType { get; private set; }
        public Guid OrderId { get; private set; }
        public int? Quantity { get; private set; }
        public int? LimitPrice { get; private set; }
        public OrderSide? Side { get; private set; }

    }
}
