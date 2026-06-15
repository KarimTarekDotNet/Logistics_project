namespace Domain.Entities.Users.Subscriptions
{
    public class UserSubscriptionUsage
    {
        public Guid Id { get; set; }

        public Guid UserSubscriptionId { get; set; }
        public UserSubscription UserSubscription { get; set; } = null!;

        public string LimitCode { get; set; } = null!;
        public decimal UsedValue { get; set; }
        public DateTimeOffset PeriodStart { get; set; }
        public DateTimeOffset PeriodEnd { get; set; }
    }
}