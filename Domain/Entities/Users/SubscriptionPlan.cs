namespace Domain.Entities.Users
{
    public class SubscriptionPlan
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;

        public decimal Price { get; set; }

        public int DurationInDays { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<UserSubscription> UserSubscriptions = new HashSet<UserSubscription>();

        public DateTimeOffset? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}