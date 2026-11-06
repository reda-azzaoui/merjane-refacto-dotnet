using Merjane.Inventory.Application.Common;

namespace Merjane.Inventory.Infrastructure.Persistence;

internal sealed class EntityFrameworkUnitOfWork(InventoryDbContext dbContext) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
