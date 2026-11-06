using Merjane.Inventory.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Merjane.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class SeasonalProductConfiguration : IEntityTypeConfiguration<SeasonalProduct>
{
    public void Configure(EntityTypeBuilder<SeasonalProduct> builder)
    {
        builder.OwnsOne(product => product.Season, season =>
        {
            season.Property(period => period.StartDate).HasColumnName("season_start_date");
            season.Property(period => period.EndDate).HasColumnName("season_end_date");
        });
        builder.Navigation(product => product.Season).IsRequired();
    }
}
