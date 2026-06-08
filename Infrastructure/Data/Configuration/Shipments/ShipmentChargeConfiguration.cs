using Domain.Entities.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.Shipments
{
    public class ShipmentChargeConfiguration : IEntityTypeConfiguration<ShipmentCharge>
    {
        public void Configure(EntityTypeBuilder<ShipmentCharge> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.TaxAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.PayerType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.ChargeType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.HasIndex(x => x.ShipmentId);

            builder.HasOne(x => x.Invoice)
                .WithMany(x => x.Charges)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class ShipmentChargeItemConfiguration : IEntityTypeConfiguration<ShipmentChargeItem>
    {
        public void Configure(EntityTypeBuilder<ShipmentChargeItem> builder)
        {
            builder.HasKey(x => new { x.ShipmentChargeId, x.ShipmentItemId });

            builder.HasOne(x => x.ShipmentCharge)
                .WithMany(x => x.ChargeItems)
                .HasForeignKey(x => x.ShipmentChargeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ShipmentItem)
                .WithMany(x => x.ChargeItems)
                .HasForeignKey(x => x.ShipmentItemId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}