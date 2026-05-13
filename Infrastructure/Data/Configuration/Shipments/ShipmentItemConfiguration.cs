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

            builder.Property(x => x.ChargeableWeight)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(x => x.GrossWeight)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(x => x.NetWeight)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(x => x.RequiredTemperatureCelsius)
                .HasPrecision(5, 2)
                .IsRequired(false);

            builder.Property(x => x.VolumeCbm)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.ShipmentId);
        }
    }
}