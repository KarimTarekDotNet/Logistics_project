using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration.User
{
    public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            builder.HasKey(sp => sp.Id);

            builder.Property(sp => sp.Title).IsRequired().HasMaxLength(100);

            builder.Property(sp => sp.Description).IsRequired().HasMaxLength(500);

            builder.Property(sp => sp.Price).IsRequired().HasColumnType("decimal(18,2)");

            builder.Property(sp => sp.DurationInDays).IsRequired();

            builder.Property(sp => sp.IsActive).IsRequired();

            builder.Property(sp => sp.CreatedAt).IsRequired();

            builder.Property(sp => sp.IsDeleted).IsRequired(false);
            builder.Property(sp => sp.DeletedAt).IsRequired(false);

            builder.Property(sp => sp.UpdatedAt).IsRequired(false);
        }
    }
    public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
    {
        public void Configure(EntityTypeBuilder<UserSubscription> builder)
        {
            builder.HasKey(usp => usp.Id);

            builder.Property(usp => usp.UserId).IsRequired();

            builder.Property(sp => sp.IsActive).IsRequired();

            builder.HasIndex(x => new { x.UserId, x.IsActive });

            builder.Property(usp => usp.StartDate).IsRequired();
            builder.Property(usp => usp.EndDate).IsRequired();

            builder.Property(usp => usp.SubscriptionPlanId).IsRequired();

            builder.Property(sp => sp.CreatedAt).IsRequired();

            builder.Property(sp => sp.IsDeleted).IsRequired(false);
            builder.Property(sp => sp.DeletedAt).IsRequired(false);

            builder.Property(sp => sp.UpdatedAt).IsRequired(false);

            builder.HasMany(x => x.Payments)
                .WithOne(x => x.UserSubscription)
                .HasForeignKey(x => x.UserSubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SubscriptionPlan)
                .WithMany(x => x.UserSubscriptions)
                .HasForeignKey(x => x.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}