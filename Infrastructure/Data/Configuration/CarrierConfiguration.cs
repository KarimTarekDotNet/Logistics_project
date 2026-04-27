using Domain.Entities.ShippingCore;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class CarrierConfiguration : IEntityTypeConfiguration<Carrier>
    {
        public void Configure(EntityTypeBuilder<Carrier> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Code)
                .HasMaxLength(10)
                .IsRequired();

            builder.HasIndex(p => p.Code).IsUnique();

            builder.HasData(
                SeedData.CarrierMaersk,
                SeedData.CarrierMSC
            );
        }
    }
}
