using Merjane.Inventory.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Merjane.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(product => product.Id);
        builder.Ignore(product => product.DomainEvents);

        builder.Property(product => product.Id).HasColumnName("id");
        builder.Property(product => product.Name).HasColumnName("name").IsRequired();
        builder.Property(product => product.AvailableQuantity).HasColumnName("available");
        builder.Property(product => product.RestockLeadTimeInDays).HasColumnName("lead_time");

        builder.HasDiscriminator<string>("type")
            .HasValue<NormalProduct>("NORMAL")
            .HasValue<SeasonalProduct>("SEASONAL")
            .HasValue<ExpirableProduct>("EXPIRABLE");
    }
}
