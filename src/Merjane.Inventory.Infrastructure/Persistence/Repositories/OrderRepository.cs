using Merjane.Inventory.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Merjane.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(InventoryDbContext dbContext) : IOrderRepository
{
    public Task<Order?> FindByIdAsync(long orderId, CancellationToken cancellationToken) =>
        dbContext.Orders.SingleOrDefaultAsync(order => order.Id == orderId, cancellationToken);
}
