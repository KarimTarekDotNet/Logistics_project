using Domain.Entities.ShippingCore;
using Infrastructure.Data.Configuration.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.Pricing
{
    public class ContainerTypeConfiguration : IEntityTypeConfiguration<ContainerType>
    {
        public void Configure(EntityTypeBuilder<ContainerType> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}
