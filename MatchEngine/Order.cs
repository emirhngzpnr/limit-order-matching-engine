namespace MatchEngine
{
    public class Order
    {
        // emir kimliği
        public Guid Id { get; }
        // emir adeti
        public int Quantity { get; }
        // limit fiyat
        public int LimitPrice { get; }
        // emir türü
        public OrderSide Side { get; }
        public Order(int quantity, int limitPrice, OrderSide side)
            :this(Guid.NewGuid(),quantity,limitPrice,side)
        {
           
        }

        public Order(Guid id, int quantity, int limitPrice, OrderSide side)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                   nameof(quantity), "Emir adedi pozitif olmalıdır.");
            }

            if (limitPrice <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(limitPrice), "Limit fiyatı pozitif olmalıdır.");
            }
            Id = id;
            Quantity = quantity;
            LimitPrice = limitPrice;
            Side = side;

        }
    }
}
