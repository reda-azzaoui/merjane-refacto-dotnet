using Merjane.Inventory.Domain.Products;
using Merjane.Inventory.Domain.Products.Events;

namespace Merjane.Inventory.Domain.UnitTests.Products;

public class ExpirableProductTests
{
    private static readonly DateTime Now = new(2026, 6, 15, 10, 0, 0);
    private static readonly DateTime Today = Now.Date;
    private const string Name = "Milk";

    private static ExpirableProduct CreateProduct(int availableQuantity, DateTime expiryDate) =>
        new(Name, availableQuantity, restockLeadTimeInDays: 7, expiryDate);

    [Fact]
    public void In_stock_product_is_sold_before_its_expiry_date()
    {
        ExpirableProduct product = CreateProduct(6, Today.AddDays(26));

        product.FulfillOneUnit(Now);

        Assert.Equal(5, product.AvailableQuantity);
        Assert.Empty(product.DomainEvents);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(18)]
    public void In_stock_product_expiring_today_after_midnight_is_still_sold_whatever_the_current_hour(int expiryHour)
    {
        ExpirableProduct product = CreateProduct(6, Today.AddHours(expiryHour));

        product.FulfillOneUnit(Now);

        Assert.Equal(5, product.AvailableQuantity);
        Assert.Empty(product.DomainEvents);
    }

    [Fact]
    public void In_stock_product_expiring_today_at_midnight_is_expired()
    {
        ExpirableProduct product = CreateProduct(6, Today);

        product.FulfillOneUnit(Now);

        Assert.Equal(0, product.AvailableQuantity);
        Assert.Equal(new ProductExpired(Name, Today), Assert.Single(product.DomainEvents));
    }

    [Fact]
    public void Expired_product_loses_its_stock_and_reports_its_expiration()
    {
        DateTime expiryDate = Today.AddDays(-2);
        ExpirableProduct product = CreateProduct(6, expiryDate);

        product.FulfillOneUnit(Now);

        Assert.Equal(0, product.AvailableQuantity);
        Assert.Equal(new ProductExpired(Name, expiryDate), Assert.Single(product.DomainEvents));
    }

    [Fact]
    public void Out_of_stock_product_reports_an_expiration_even_before_its_expiry_date()
    {
        DateTime expiryDate = Today.AddDays(26);
        ExpirableProduct product = CreateProduct(0, expiryDate);

        product.FulfillOneUnit(Now);

        Assert.Equal(0, product.AvailableQuantity);
        Assert.Equal(new ProductExpired(Name, expiryDate), Assert.Single(product.DomainEvents));
    }
}
