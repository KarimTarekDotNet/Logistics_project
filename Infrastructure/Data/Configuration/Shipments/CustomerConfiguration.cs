using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.Shipments
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ApplicationUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.NationalId)
                .HasMaxLength(14);

            builder.Property(x => x.CompanyName)
                .HasMaxLength(150);

            builder.Property(x => x.TaxNumber)
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.ApplicationUserId)
                .IsUnique();

            builder.Property(x => x.CountryCode)
                .HasMaxLength(5);

            builder.HasIndex(x => x.NationalId)
                .IsUnique()
                .HasFilter("[NationalId] IS NOT NULL");

            builder.HasIndex(x => x.TaxNumber)
                .IsUnique()
                .HasFilter("[TaxNumber] IS NOT NULL");

            // One-to-One with ApplicationUser
            builder.HasOne(u => u.ApplicationUser)
                .WithOne(c => c.CustomerProfile)
                .HasForeignKey<Customer>(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships
            builder.HasMany(x => x.Shipments)
                .WithOne(x => x.Customer)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
