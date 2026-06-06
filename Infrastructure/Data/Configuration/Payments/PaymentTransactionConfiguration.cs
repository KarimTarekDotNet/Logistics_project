using Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.Payments
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Currency)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.ProviderTransactionId)
                .HasMaxLength(200);

            builder.Property(x => x.ProviderIntentionId)
                .HasMaxLength(200);

            builder.Property(x => x.ProviderOrderId)
                .HasMaxLength(200);

            builder.Property(x => x.Method)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.Provider)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasOne(x => x.Invoice)
                .WithMany(x => x.PaymentTransactions)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.InvoiceId);

            builder.HasIndex(x => x.ProviderTransactionId).HasFilter("[ProviderTransactionId] IS NOT NULL").IsUnique();

            builder.HasIndex(x => new { x.InvoiceId, x.CreatedAt });
        }
    }
}
