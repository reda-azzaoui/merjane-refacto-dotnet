namespace Merjane.Inventory.Domain.Products;

/// <summary>Sold while in stock; otherwise a restock delay is announced, if there is one.</summary>
public sealed class NormalProduct : Product
{
    public NormalProduct(string name, int availableQuantity, int restockLeadTimeInDays)
        : base(name, availableQuantity, restockLeadTimeInDays)
    {
    }

    private NormalProduct()
    {
    }

    public override void FulfillOneUnit(DateTime now)
    {
        if (IsInStock)
        {
            RemoveOneUnitFromStock();
            return;
        }

        if (RestockLeadTimeInDays > 0)
        {
            AnnounceRestockDelay();
        }
    }
}
