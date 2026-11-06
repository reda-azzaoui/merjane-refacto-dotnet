using Merjane.Inventory.Domain.Orders;

namespace Merjane.Inventory.Domain.UnitTests.Orders;

public class OrderTests
{
    [Fact]
    public void Order_keeps_one_line_per_product()
    {
        Order order = new([1, 2, 2]);

        Assert.Equal([1L, 2L], order.Lines.Select(line => line.ProductId));
    }

    [Fact]
    public void Order_can_have_no_product()
    {
        Order order = new([]);

        Assert.Empty(order.Lines);
    }
}
