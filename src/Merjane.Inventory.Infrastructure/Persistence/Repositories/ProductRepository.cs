using Merjane.Inventory.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Merjane.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(InventoryDbContext dbContext) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> ListByIdsAsync(IReadOnlyCollection<long> productIds, CancellationToken cancellationToken) =>
        await dbContext.Products
            .Where(product => productIds.Contains(product.Id))
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);
}
