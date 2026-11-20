using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine.Persistence
{
    public sealed record StoredOrderCommand(
                            long SequenceNumber,
                            OrderCommand Command
        );
    
}
