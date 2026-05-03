using Domain.Entities.Pricing.PricingEngine;
using Infrastructure.Data.Configuration.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.Pricing
{
    public class RateConfiguration : IEntityTypeConfiguration<Rate>
    {
        public void Configure(EntityTypeBuilder<Rate> builder)
        {
            builder.HasKey(r => r.Id);

            builder.HasOne(r => r.Carrier)
                .WithMany(c => c.Rates)
                .HasForeignKey(r => r.CarrierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Route)
                .WithMany(c => c.Rates)
                .HasForeignKey(r => r.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ContainerType)
                .WithMany()
                .HasForeignKey(r => r.ContainerTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(r => r.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
    
            builder.Property(r => r.Currency)
                .HasMaxLength(4)
                .IsRequired();
    
            builder.Property(r => r.ValidFrom)
                .IsRequired();
    
            builder.Property(r => r.ValidTo)
                .IsRequired();
    
            builder.Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            builder.Property(r => r.IsActive);

            builder.HasIndex(r => r.CarrierId);
            builder.HasIndex(r => r.RouteId);
            builder.HasIndex(r => r.ContainerTypeId);
            builder.HasIndex(r => r.IsActive);
            builder.HasIndex(r => r.ValidTo);
        }
    }
}
