using Domain.Entities.Shipments;
using Infrastructure.Data.Configuration.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.Shipments
{
    public class ShipmentStatusHistoryConfiguration : IEntityTypeConfiguration<ShipmentStatusHistory>
    {
        public void Configure(EntityTypeBuilder<ShipmentStatusHistory> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FromStatus)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.ToStatus)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.ChangedAt)
                .IsRequired();

            builder.Property(x => x.ChangedBy)
                .HasMaxLength(100);

            builder.Property(x => x.ChangedByUserId)
                .HasMaxLength(100);

            builder.Property(x => x.ChangedByRole)
                .HasMaxLength(100);

            builder.Property(x => x.Reason)
                .HasMaxLength(500);

            builder.HasIndex(x => x.ShipmentId);

            builder.HasIndex(x => x.ChangedAt);

            builder.HasIndex(x => new { x.ShipmentId, x.ChangedAt });

            builder.HasIndex(x => new { x.ToStatus, x.ChangedAt });
        }
    }
}