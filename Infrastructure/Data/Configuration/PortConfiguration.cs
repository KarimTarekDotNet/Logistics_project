using Domain.Entities.ShippingCore;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class PortConfiguration : IEntityTypeConfiguration<Port>
    {
        public void Configure(EntityTypeBuilder<Port> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Country)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Code)
                .HasMaxLength(10)
                .IsRequired();

            builder.HasIndex(p => p.Code).IsUnique();

            builder.HasData(
                SeedData.PortShanghai,
                SeedData.PortRotterdam,
                SeedData.PortDubai
            );
        }
    }
}
