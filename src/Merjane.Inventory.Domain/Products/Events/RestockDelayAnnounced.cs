using Merjane.Inventory.Domain.Common;

namespace Merjane.Inventory.Domain.Products.Events;

public sealed record RestockDelayAnnounced(string ProductName, int RestockLeadTimeInDays) : IDomainEvent;
