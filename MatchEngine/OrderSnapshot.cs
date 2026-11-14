using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine
{
    public record OrderSnapshot(
        Order Order,
        int RemainingQuantity
        );
    
}
