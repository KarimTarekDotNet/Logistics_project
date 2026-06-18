using Domain.Entities.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class AuditConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.EntityName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.EntityId)
                .IsRequired();

            builder.Property(x => x.OldValues)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.NewValues)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.IpAddress)
                .HasMaxLength(45);
            // IPv4 = 15 chars
            // IPv6 = 39 chars
            // 45 gives some margin

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.UserId);

            builder.HasIndex(x => new
            {
                x.EntityName,
                x.EntityId
            });

            builder.HasIndex(x => x.CreatedAt);
        }
    }
}
