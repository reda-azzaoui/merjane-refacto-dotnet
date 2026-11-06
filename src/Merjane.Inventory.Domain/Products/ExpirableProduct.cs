using Merjane.Inventory.Domain.Products.Events;

namespace Merjane.Inventory.Domain.Products;

/// <summary>Sold while in stock and before its expiry date; in every other case its stock is withdrawn and an expiration is reported.</summary>
public sealed class ExpirableProduct : Product
{
    public ExpirableProduct(string name, int availableQuantity, int restockLeadTimeInDays, DateTime expiryDate)
        : base(name, availableQuantity, restockLeadTimeInDays)
    {
        ExpiryDate = expiryDate;
    }

    private ExpirableProduct()
    {
    }

    public DateTime ExpiryDate { get; private set; }

    public override void FulfillOneUnit(DateTime now)
    {
        if (IsInStock && ExpiryDate > now.Date)
        {
            RemoveOneUnitFromStock();
            return;
        }

        RemoveAllUnitsFromStock();
        RaiseDomainEvent(new ProductExpired(Name, ExpiryDate));
    }
}
