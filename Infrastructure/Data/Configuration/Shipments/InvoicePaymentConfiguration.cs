using Domain.Entities.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.Shipments
{
    public class InvoicePaymentConfiguration : IEntityTypeConfiguration<InvoicePayment>
    {
        public void Configure(EntityTypeBuilder<InvoicePayment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Currency)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.TransactionId)
                .HasMaxLength(200);

            builder.Property(x => x.ReferenceNumber)
                .HasMaxLength(200);

            builder.Property(x => x.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.PaymentProvider)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasOne(x => x.Invoice)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.InvoiceId);

            builder.HasIndex(x => x.TransactionId).HasFilter("[TransactionId] IS NOT NULL").IsUnique();

            builder.HasIndex(x => new { x.InvoiceId, x.CreatedAt });
        }
    }
}
