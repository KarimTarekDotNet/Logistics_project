using Domain.Entities.ShippingCore;
using Infrastructure.Data.Configuration.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.Pricing
{
    public class RouteConfiguration : IEntityTypeConfiguration<Route>
    {
        public void Configure(EntityTypeBuilder<Route> builder)
        {
            builder.HasKey(r => r.Id);

            builder.HasOne(r => r.FromPort)
                .WithMany()
                .HasForeignKey(r => r.FromPortId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ToPort)
                .WithMany()
                .HasForeignKey(r => r.ToPortId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => r.FromPortId);
            builder.HasIndex(r => r.ToPortId);
            builder.HasIndex(r => new { r.FromPortId, r.ToPortId }).IsUnique();
        }
    }
}
