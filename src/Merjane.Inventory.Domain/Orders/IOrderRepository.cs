namespace Merjane.Inventory.Domain.Orders;

public interface IOrderRepository
{
    Task<Order?> FindByIdAsync(long orderId, CancellationToken cancellationToken);
}
