namespace Merjane.Inventory.Domain.Orders;

/// <summary>Reference to an ordered product, held by identity because a product is its own aggregate.</summary>
public sealed class OrderLine
{
    internal OrderLine(long productId) => ProductId = productId;

    public long ProductId { get; private set; }
}
