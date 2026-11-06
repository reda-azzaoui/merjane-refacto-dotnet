using Merjane.Inventory.Domain.Common;
using Merjane.Inventory.Domain.Products.Events;

namespace Merjane.Inventory.Domain.Products;

/// <summary>
/// Sellable item. Each product family implements its own availability rules in
/// <see cref="FulfillOneUnit"/>, so adding a family never changes the existing ones.
/// </summary>
public abstract class Product : AggregateRoot
{
    protected Product(string name, int availableQuantity, int restockLeadTimeInDays)
    {
        Name = name;
        AvailableQuantity = availableQuantity;
        RestockLeadTimeInDays = restockLeadTimeInDays;
    }

    private protected Product()
    {
    }

    public long Id { get; private set; }

    public string Name
    {
        get;
        private set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value;
        }
    } = null!;

    public int AvailableQuantity
    {
        get;
        private set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            field = value;
        }
    }

    public int RestockLeadTimeInDays
    {
        get;
        private set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            field = value;
        }
    }

    protected bool IsInStock => AvailableQuantity > 0;

    /// <summary>Applies the outcome of one ordered unit at the moment <paramref name="now"/> and raises the matching events.</summary>
    public abstract void FulfillOneUnit(DateTime now);

    protected void RemoveOneUnitFromStock() => AvailableQuantity--;

    protected void RemoveAllUnitsFromStock() => AvailableQuantity = 0;

    protected void AnnounceRestockDelay() => RaiseDomainEvent(new RestockDelayAnnounced(Name, RestockLeadTimeInDays));
}
