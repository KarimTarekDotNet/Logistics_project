using Domain.Entities.Shipments;
using Infrastructure.Data.Configuration.Seeding;
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
}