namespace Domain.Entities.Users.Subscriptions
{
    public class SubscriptionPlan
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;

        public decimal Price { get; set; }
        public string Currency { get; set; } = "EGP";

        public int DurationInDays { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<UserSubscription> UserSubscriptions { get; set; } = new HashSet<UserSubscription>();
        public ICollection<SubscriptionFeature> Features { get; set; } = new HashSet<SubscriptionFeature>();

        public DateTimeOffset? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}