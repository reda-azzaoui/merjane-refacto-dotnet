using Merjane.Inventory.Domain.Common;

namespace Merjane.Inventory.Domain.Products.Events;

public sealed record ProductUnavailable(string ProductName) : IDomainEvent;
