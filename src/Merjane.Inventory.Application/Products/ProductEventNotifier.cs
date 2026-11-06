using System.Diagnostics;
using Merjane.Inventory.Domain.Common;
using Merjane.Inventory.Domain.Products;
using Merjane.Inventory.Domain.Products.Events;
using Refacto.DotNet.Controllers.Services;

namespace Merjane.Inventory.Application.Products;

/// <summary>Turns the domain events raised by products into customer notifications.</summary>
public sealed class ProductEventNotifier(INotificationService notificationService)
{
    public void NotifyCustomers(IEnumerable<Product> products)
    {
        ArgumentNullException.ThrowIfNull(products);

        foreach (Product product in products)
        {
            foreach (IDomainEvent domainEvent in product.DomainEvents)
            {
                Notify(domainEvent);
            }

            product.ClearDomainEvents();
        }
    }

    private void Notify(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case RestockDelayAnnounced delay:
                notificationService.SendDelayNotification(delay.RestockLeadTimeInDays, delay.ProductName);
                break;
            case ProductUnavailable unavailable:
                notificationService.SendOutOfStockNotification(unavailable.ProductName);
                break;
            case ProductExpired expired:
                notificationService.SendExpirationNotification(expired.ProductName, expired.ExpiryDate);
                break;
            default:
                throw new UnreachableException($"Unhandled domain event {domainEvent.GetType().Name}.");
        }
    }
}
