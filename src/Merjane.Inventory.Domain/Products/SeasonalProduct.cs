using Merjane.Inventory.Domain.Products.Events;

namespace Merjane.Inventory.Domain.Products;

/// <summary>
/// Sold while its season is open and it is in stock. Otherwise, the product becomes
/// unavailable if a restock would arrive after the season, or if the season has not
/// started yet; in all other cases a restock delay is announced.
/// </summary>
public sealed class SeasonalProduct : Product
{
    public SeasonalProduct(string name, int availableQuantity, int restockLeadTimeInDays, SeasonPeriod season)
        : base(name, availableQuantity, restockLeadTimeInDays)
    {
        ArgumentNullException.ThrowIfNull(season);
        Season = season;
    }

    private SeasonalProduct()
    {
        Season = null!;
    }

    public SeasonPeriod Season { get; private set; }

    public override void FulfillOneUnit(DateTime now)
    {
        if (Season.IsOpenOn(now.Date) && IsInStock)
        {
            RemoveOneUnitFromStock();
            return;
        }

        DateTime restockMoment = now.AddDays(RestockLeadTimeInDays);
        if (Season.IsOverAt(restockMoment))
        {
            RemoveAllUnitsFromStock();
            RaiseDomainEvent(new ProductUnavailable(Name));
            return;
        }

        if (Season.HasNotStartedAt(now))
        {
            RaiseDomainEvent(new ProductUnavailable(Name));
            return;
        }

        AnnounceRestockDelay();
    }
}
