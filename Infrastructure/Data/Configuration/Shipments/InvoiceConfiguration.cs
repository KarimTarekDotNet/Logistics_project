using Domain.Entities.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.Shipments
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.CancellationReason)
                .IsRequired(false)
                .HasMaxLength(300);
            
            builder.Property(x => x.CancelledByUserId)
                .IsRequired(false)
                .HasMaxLength(150);

            builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(x => x.SubTotal)
                .HasPrecision(18, 2);

            builder.Property(x => x.NetShipmentPrice)
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.TaxAmount)
                .HasPrecision(18, 4);

            builder.Property(x => x.PaymentStatus)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.PayerType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.IssuedAt)
                .IsRequired();

            builder.Property(x => x.DueDate)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.CancelledAt)
                .IsRequired(false);

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            builder.HasIndex(x => x.ShipmentId);

            builder.HasIndex(x => x.InvoiceNumber)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(x => x.PaymentStatus);

            builder.HasIndex(x => x.DueDate);

            builder.HasIndex(x => new { x.ShipmentId, x.PaymentStatus });

            builder.HasIndex(x => new { x.IsDeleted, x.PaymentStatus, x.DueDate });
        }
    }
}
