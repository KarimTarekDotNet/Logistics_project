using Domain.Entities.Pricing.Quotation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.Pricing
{
    public class QuoteRequestConfiguration : IEntityTypeConfiguration<QuoteRequest>
    {
        public void Configure(EntityTypeBuilder<QuoteRequest> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestedRatePrice)
                .HasPrecision(18, 2);

            builder.Property(x => x.RequestedGrossWeightKg)
                .HasPrecision(18, 3);

            builder.Property(x => x.RequestedNetWeightKg)
                .HasPrecision(18, 3);

            builder.Property(x => x.RequestedVolumeCbm)
                .HasPrecision(18, 3);

            builder.Property(x => x.RequiredTemperatureCelsius)
                .HasPrecision(5, 2);

            builder.Property(x => x.Status)
                .HasConversion<string>();
        }
    }
}
