using Merjane.Inventory.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Merjane.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class ExpirableProductConfiguration : IEntityTypeConfiguration<ExpirableProduct>
{
    public void Configure(EntityTypeBuilder<ExpirableProduct> builder) =>
        builder.Property(product => product.ExpiryDate).HasColumnName("expiry_date");
}
