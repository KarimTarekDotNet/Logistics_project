using Domain.Entities.ShippingCore;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class ContainerTypeConfiguration : IEntityTypeConfiguration<ContainerType>
    {
        public void Configure(EntityTypeBuilder<ContainerType> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasData(
                SeedData.Container20Ft,
                SeedData.Container40Ft,
                SeedData.Container40HQ
            );
        }
    }
}
