using Merjane.Inventory.Domain.Orders;
using Merjane.Inventory.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Merjane.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(order => order.Id);
        builder.Ignore(order => order.DomainEvents);

        builder.Property(order => order.Id).HasColumnName("id");

        builder.OwnsMany(order => order.Lines, line =>
        {
            line.ToTable("order_lines");
            line.WithOwner().HasForeignKey("order_id");
            line.Property(orderLine => orderLine.ProductId).HasColumnName("product_id");
            line.HasKey("order_id", nameof(OrderLine.ProductId));
            line.HasOne<Product>().WithMany().HasForeignKey(orderLine => orderLine.ProductId);
        });
    }
}
