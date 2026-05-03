using Domain.Entities.Shipments;
using Infrastructure.Data.Configuration.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.Shipments
{
    public class ShipmentConfiguration : IEntityTypeConfiguration<Domain.Entities.Shipments.Shipment>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Shipments.Shipment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AgreedPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.Quote)
                .WithOne(x => x.Shipment)
                .HasForeignKey<Domain.Entities.Shipments.Shipment>(x => x.QuoteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Route)
                .WithMany(x => x.Shipments)
                .HasForeignKey(x => x.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ContainerType)
                .WithMany(x => x.Shipments)
                .HasForeignKey(x => x.ContainerTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Carrier)
                .WithMany(x => x.Shipments)
                .HasForeignKey(x => x.CarrierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.Shipment)
                .HasForeignKey(x => x.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Charges)
                .WithOne(x => x.Shipment)
                .HasForeignKey(x => x.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.StatusHistory)
                .WithOne(x => x.Shipment)
                .HasForeignKey(x => x.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.QuoteId).IsUnique();

            builder.HasIndex(x => x.Currency);

            builder.HasIndex(x => x.Status);

            builder.HasIndex(x => x.CarrierId);

            builder.HasIndex(x => x.RouteId);

            builder.HasIndex(x => x.ContainerTypeId);

            builder.HasIndex(x => x.CreatedAt);

            builder.HasIndex(x => new { x.Status, x.CreatedAt });

            builder.HasIndex(x => new { x.CarrierId, x.Status });

            builder.HasIndex(x => new { x.RouteId, x.ContainerTypeId, x.Status });

            builder.HasIndex(x => new { x.CarrierId, x.RouteId, x.ContainerTypeId });
        }
    }
}