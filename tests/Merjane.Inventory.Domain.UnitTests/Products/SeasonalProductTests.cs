using Merjane.Inventory.Domain.Products;
using Merjane.Inventory.Domain.Products.Events;

namespace Merjane.Inventory.Domain.UnitTests.Products;

public class SeasonalProductTests
{
    private static readonly DateTime Now = new(2026, 6, 15, 10, 0, 0);
    private static readonly DateTime Today = Now.Date;
    private const string Name = "Watermelon";

    private static SeasonalProduct CreateProduct(int availableQuantity, int restockLeadTimeInDays, DateTime seasonStart, DateTime seasonEnd) =>
        new(Name, availableQuantity, restockLeadTimeInDays, new SeasonPeriod(seasonStart, seasonEnd));

    [Fact]
    public void In_stock_product_is_sold_during_its_season()
    {
        SeasonalProduct product = CreateProduct(5, 3, Today.AddDays(-2), Today.AddDays(58));

        product.FulfillOneUnit(Now);

        Assert.Equal(4, product.AvailableQuantity);
        Assert.Empty(product.DomainEvents);
    }

    [Fact]
    public void Out_of_stock_product_announces_a_delay_when_restock_arrives_during_the_season()
    {
        SeasonalProduct product = CreateProduct(0, 3, Today.AddDays(-2), Today.AddDays(58));

        product.FulfillOneUnit(Now);

        Assert.Equal(new RestockDelayAnnounced(Name, 3), Assert.Single(product.DomainEvents));
    }

    [Fact]
    public void Out_of_stock_product_announces_a_delay_when_restock_arrives_exactly_at_the_end_of_the_season()
    {
        SeasonalProduct product = CreateProduct(0, 3, Today.AddDays(-2), Now.AddDays(3));

        product.FulfillOneUnit(Now);

        Assert.Equal(new RestockDelayAnnounced(Name, 3), Assert.Single(product.DomainEvents));
    }

    [Fact]
    public void Out_of_stock_product_is_unavailable_when_restock_arrives_after_the_season()
    {
        SeasonalProduct product = CreateProduct(0, 30, Today.AddDays(-2), Today.AddDays(5));

        product.FulfillOneUnit(Now);

        Assert.Equal(0, product.AvailableQuantity);
        Assert.Equal(new ProductUnavailable(Name), Assert.Single(product.DomainEvents));
    }

    [Fact]
    public void Out_of_stock_product_without_lead_time_announces_a_delay_of_zero_days()
    {
        SeasonalProduct product = CreateProduct(0, 0, Today.AddDays(-2), Today.AddDays(5));

        product.FulfillOneUnit(Now);

        Assert.Equal(new RestockDelayAnnounced(Name, 0), Assert.Single(product.DomainEvents));
    }

    [Fact]
    public void Product_is_unavailable_before_its_season_and_keeps_its_stock()
    {
        SeasonalProduct product = CreateProduct(5, 15, Today.AddDays(180), Today.AddDays(240));

        product.FulfillOneUnit(Now);

        Assert.Equal(5, product.AvailableQuantity);
        Assert.Equal(new ProductUnavailable(Name), Assert.Single(product.DomainEvents));
    }

    [Fact]
    public void Product_loses_its_stock_before_its_season_when_lead_time_goes_past_the_season()
    {
        SeasonalProduct product = CreateProduct(5, 30, Today.AddDays(10), Today.AddDays(20));

        product.FulfillOneUnit(Now);

        Assert.Equal(0, product.AvailableQuantity);
        Assert.Equal(new ProductUnavailable(Name), Assert.Single(product.DomainEvents));
    }

    [Fact]
    public void Product_loses_its_stock_after_its_season()
    {
        SeasonalProduct product = CreateProduct(5, 3, Today.AddDays(-30), Today.AddDays(-1));

        product.FulfillOneUnit(Now);

        Assert.Equal(0, product.AvailableQuantity);
        Assert.Equal(new ProductUnavailable(Name), Assert.Single(product.DomainEvents));
    }

    [Fact]
    public void In_stock_product_is_not_sold_on_the_first_day_of_its_season_and_announces_a_delay()
    {
        SeasonalProduct product = CreateProduct(5, 3, Today, Today.AddDays(30));

        product.FulfillOneUnit(Now);

        Assert.Equal(5, product.AvailableQuantity);
        Assert.Equal(new RestockDelayAnnounced(Name, 3), Assert.Single(product.DomainEvents));
    }

    [Fact]
    public void In_stock_product_loses_its_stock_on_the_last_day_of_its_season()
    {
        SeasonalProduct product = CreateProduct(5, 3, Today.AddDays(-30), Today);

        product.FulfillOneUnit(Now);

        Assert.Equal(0, product.AvailableQuantity);
        Assert.Equal(new ProductUnavailable(Name), Assert.Single(product.DomainEvents));
    }

    [Fact]
    public void Season_cannot_end_before_it_starts()
    {
        Assert.Throws<ArgumentException>(() => new SeasonPeriod(Today, Today.AddDays(-1)));
    }
}
