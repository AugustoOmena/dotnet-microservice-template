using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.Domain.Items;

namespace Service.Infrastructure.Persistence.Configurations;

internal sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}