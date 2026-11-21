using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine.Persistence
{
    public interface IOrderCommandStore
    {
        Task AppendAsync(OrderCommand command);

        Task<IReadOnlyList<StoredOrderCommand>> ReadBatchAsync(
            long afterSequenceNumber,
            int batchSize);
    }
}
