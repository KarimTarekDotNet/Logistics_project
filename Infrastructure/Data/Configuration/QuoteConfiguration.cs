using Domain.Entities.Pricing.Quotation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
    {
        public void Configure(EntityTypeBuilder<Quote> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(p => p.CustomerName)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasOne(r => r.Route)
                .WithMany()
                .HasForeignKey(r => r.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x =>x.ContainerType)
                .WithMany()
                .HasForeignKey(q => q.ContainerTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.FinalPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Currency)
                .HasMaxLength(4)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            builder.HasMany(x => x.Items)
                .WithOne(i => i.Quote)
                .HasForeignKey(i => i.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(r => r.RouteId);
            builder.HasIndex(r => r.ContainerTypeId);
            builder.HasIndex(r => r.CreatedAt);

            builder.HasData(
                SeedData.QuoteAlpha,
                SeedData.QuoteBeta
            );
        }
    }
}
