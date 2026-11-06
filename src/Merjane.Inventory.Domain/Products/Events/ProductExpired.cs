using Merjane.Inventory.Domain.Common;

namespace Merjane.Inventory.Domain.Products.Events;

public sealed record ProductExpired(string ProductName, DateTime ExpiryDate) : IDomainEvent;
