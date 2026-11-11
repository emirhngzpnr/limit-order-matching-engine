using System.Security.Cryptography.X509Certificates;

namespace MatchEngine.Tests;

public class OrderTests
{
   // adet 0 kontrolü
    [Fact]
    public void Order_RejectsZeroQuantity()
    {
        Assert.Throws<ArgumentOutOfRangeException>
            (
            () => new Order(0,100,OrderSide.Buy)
            );

    }
    // adet başarı kontrolü
    [Fact]
    public void Order_StoresValidQuantity()
    {
        var order = new Order(5,100,OrderSide.Buy);
        Assert.Equal(5, order.Quantity);
    }

    [Fact]
    public void Order_RejectsZeroLimitPrice()
    {
        Assert.Throws<ArgumentOutOfRangeException>
            (
            () => new Order(5, 0,OrderSide.Buy)
            );
    }

    [Fact]
    public void Order_StoresValidLimitPrice()
    {
        var order = new Order(5, 100,OrderSide.Buy);
        Assert.Equal(100, order.LimitPrice);
    }

    [Fact]
    public void Order_StoresSellSide()
    {
        var order = new Order(5, 100, OrderSide.Sell);
        Assert.Equal(OrderSide.Sell, order.Side);
    }

}
