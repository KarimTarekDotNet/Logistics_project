using Domain.Entities.Pricing.Quotation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class QuoteItemConfiguration : IEntityTypeConfiguration<QuoteItem>
    {
        public void Configure(EntityTypeBuilder<QuoteItem> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(p => p.Description)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasIndex(q => q.QuoteId);

            builder.HasData(
                SeedData.QuoteAlphaFreight,
                SeedData.QuoteAlphaSurcharge,
                SeedData.QuoteBetaFreight,
                SeedData.QuoteBetaSurcharge
            );
        }
    }
}
