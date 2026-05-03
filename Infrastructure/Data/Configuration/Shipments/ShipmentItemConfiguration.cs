using Domain.Entities.Shipments;
using Infrastructure.Data.Configuration.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.Shipments
{
    public class ShipmentItemConfiguration : IEntityTypeConfiguration<ShipmentItem>
    {
        public void Configure(EntityTypeBuilder<ShipmentItem> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.Property(x => x.Weight)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.ShipmentId);
        }
    }
}