using Merjane.Inventory.Domain.Common;

namespace Merjane.Inventory.Domain.Orders;

public sealed class Order : AggregateRoot
{
    private readonly List<OrderLine> _lines = [];

    public Order(IEnumerable<long> productIds)
    {
        ArgumentNullException.ThrowIfNull(productIds);

        _lines = [.. productIds.Distinct().Select(productId => new OrderLine(productId))];
    }

    private Order()
    {
    }

    public long Id { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines;
}
