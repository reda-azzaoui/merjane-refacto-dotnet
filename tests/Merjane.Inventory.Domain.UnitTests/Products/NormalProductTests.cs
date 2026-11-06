using Merjane.Inventory.Domain.Products;
using Merjane.Inventory.Domain.Products.Events;

namespace Merjane.Inventory.Domain.UnitTests.Products;

public class NormalProductTests
{
    private static readonly DateTime Now = new(2026, 6, 15, 10, 0, 0);

    [Fact]
    public void In_stock_product_loses_one_unit_without_any_event()
    {
        NormalProduct product = new("USB Cable", availableQuantity: 30, restockLeadTimeInDays: 15);

        product.FulfillOneUnit(Now);

        Assert.Equal(29, product.AvailableQuantity);
        Assert.Empty(product.DomainEvents);
    }

    [Fact]
    public void Out_of_stock_product_announces_a_restock_delay()
    {
        NormalProduct product = new("USB Dongle", availableQuantity: 0, restockLeadTimeInDays: 10);

        product.FulfillOneUnit(Now);

        Assert.Equal(0, product.AvailableQuantity);
        Assert.Equal(new RestockDelayAnnounced("USB Dongle", 10), Assert.Single(product.DomainEvents));
    }

    [Fact]
    public void Out_of_stock_product_without_lead_time_announces_nothing()
    {
        NormalProduct product = new("USB Dongle", availableQuantity: 0, restockLeadTimeInDays: 0);

        product.FulfillOneUnit(Now);

        Assert.Empty(product.DomainEvents);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Product_requires_a_name(string name)
    {
        Assert.ThrowsAny<ArgumentException>(() => new NormalProduct(name, 1, 1));
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    public void Product_rejects_negative_quantities(int availableQuantity, int restockLeadTimeInDays)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new NormalProduct("USB Cable", availableQuantity, restockLeadTimeInDays));
    }
}
