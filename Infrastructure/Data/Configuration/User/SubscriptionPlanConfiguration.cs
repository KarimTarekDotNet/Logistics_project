using Domain.Entities.Users.Subscriptions;
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

            builder.Property(sp => sp.IsDeleted).IsRequired();
            builder.Property(sp => sp.DeletedAt).IsRequired(false);

            builder.Property(sp => sp.UpdatedAt).IsRequired(false);

            builder.Property(sp => sp.Currency)
            .IsRequired()
            .HasMaxLength(10);

            builder.HasMany(x => x.Features)
                .WithMany(x => x.SubscriptionPlans);
        }
    }

    public class SubscriptionFeatureConfiguration : IEntityTypeConfiguration<SubscriptionFeature>
    {
        public void Configure(EntityTypeBuilder<SubscriptionFeature> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FeatureCode)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.FeatureName)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(x => x.FeatureCode)
                .IsUnique();
        }
    }
    public class SubscriptionPlanLimitConfiguration : IEntityTypeConfiguration<SubscriptionPlanLimit>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlanLimit> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LimitCodeSubscription)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.LimitMaxValue)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.HasIndex(x => new
            {
                x.SubscriptionFeatureId,
                x.LimitCodeSubscription
            })
            .IsUnique();

            builder.HasOne(x => x.SubscriptionFeature)
                .WithMany(x => x.PlanLimits)
                .HasForeignKey(x => x.SubscriptionFeatureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
    {
        public void Configure(EntityTypeBuilder<UserSubscription> builder)
        {
            builder.HasKey(usp => usp.Id);

            builder.Property(usp => usp.UserId).IsRequired();

            builder.Property(sp => sp.IsActive).IsRequired();

            builder.HasIndex(x => new { x.UserId, x.IsActive }).HasFilter("[IsActive] = 1");

            builder.Property(usp => usp.StartDate).IsRequired();
            builder.Property(usp => usp.EndDate).IsRequired();

            builder.Property(usp => usp.SubscriptionPlanId).IsRequired();

            builder.Property(sp => sp.CreatedAt).IsRequired();

            builder.Property(sp => sp.IsDeleted).IsRequired();
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

    public class UserSubscriptionUsageConfiguration : IEntityTypeConfiguration<UserSubscriptionUsage>
    {
        public void Configure(EntityTypeBuilder<UserSubscriptionUsage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LimitCode)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.UsedValue)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.PeriodStart)
                .IsRequired();

            builder.Property(x => x.PeriodEnd)
                .IsRequired();

            builder.HasOne(x => x.UserSubscription)
                .WithMany(x => x.Usages)
                .HasForeignKey(x => x.UserSubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new
            {
                x.UserSubscriptionId,
                x.LimitCode,
                x.PeriodStart,
                x.PeriodEnd
            }).IsUnique();
        }
    }
}