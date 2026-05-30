using Domain.Entities.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.Shipments
{
    public class ShipmentChargeRuleConfiguration : IEntityTypeConfiguration<ShipmentChargeRule>
    {
        public void Configure(EntityTypeBuilder<ShipmentChargeRule> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Value)
                .HasPrecision(18, 4);
        }
    }
}
