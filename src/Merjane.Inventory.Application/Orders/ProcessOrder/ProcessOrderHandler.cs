using Merjane.Inventory.Application.Common;
using Merjane.Inventory.Application.Products;
using Merjane.Inventory.Domain.Orders;
using Merjane.Inventory.Domain.Products;
using Microsoft.Extensions.Logging;

namespace Merjane.Inventory.Application.Orders.ProcessOrder;

/// <summary>Fulfills every product of an order, saves the result, then notifies customers.</summary>
public sealed partial class ProcessOrderHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ProductEventNotifier productEventNotifier,
    TimeProvider timeProvider,
    ILogger<ProcessOrderHandler> logger)
{
    public async Task<ProcessOrderOutcome> HandleAsync(long orderId, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.FindByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return ProcessOrderOutcome.OrderNotFound;
        }

        long[] productIds = [.. order.Lines.Select(line => line.ProductId)];
        IReadOnlyList<Product> products = await productRepository.ListByIdsAsync(productIds, cancellationToken);

        DateTime now = timeProvider.GetLocalNow().DateTime;
        foreach (Product product in products)
        {
            product.FulfillOneUnit(now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        productEventNotifier.NotifyCustomers(products);
        LogOrderProcessed(logger, orderId, products.Count);

        return ProcessOrderOutcome.Processed;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} processed with {ProductCount} products.")]
    private static partial void LogOrderProcessed(ILogger logger, long orderId, int productCount);
}
