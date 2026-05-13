using Domain.Entities.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Shipments
{
    public class ShipmentDocumentConfiguration : IEntityTypeConfiguration<ShipmentDocument>
    {
        public void Configure(EntityTypeBuilder<ShipmentDocument> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(260);

            builder.Property(x => x.StoragePath)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.ContentType)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.UploadedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.UploadedAt)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(x => x.ShipmentId);

            builder.HasIndex(x => x.Type);

            builder.HasIndex(x => x.UploadedAt);

            builder.HasIndex(x => x.UploadedByUserId);

            builder.HasIndex(x => new { x.ShipmentId, x.Type });

            builder.HasIndex(x => new { x.ShipmentId, x.UploadedAt });

            builder.HasIndex(x => x.IntegrationMessageId)
                .HasFilter("[IntegrationMessageId] IS NOT NULL");

            builder.HasIndex(x => x.IsDeleted);
        }
    }
}