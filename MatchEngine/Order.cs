namespace MatchEngine
{
    public class Order
    {
        // emir adeti
        public int Quantity { get; }
        public int LimitPrice { get; }
        public OrderSide Side { get; }
        public Order(int quantity, int limitPrice, OrderSide side)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                   nameof(quantity), "Emir adedi pozitif olmalıdır.");
            }

            if(limitPrice <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(limitPrice), "Limit fiyatı pozitif olmalıdır.");
            }

            Quantity = quantity;
            LimitPrice = limitPrice;
            Side = side;
        }

       
        
    }
}
