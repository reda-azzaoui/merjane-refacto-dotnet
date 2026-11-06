namespace Merjane.Inventory.Domain.Products;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> ListByIdsAsync(IReadOnlyCollection<long> productIds, CancellationToken cancellationToken);
}
