namespace Domain.Entities.Users.Subscriptions
{
    public class SubscriptionPlanLimit
    {
        public Guid Id { get; set; }

        public Guid SubscriptionFeatureId { get; set; }
        public SubscriptionFeature SubscriptionFeature { get; set; } = null!;

        public string LimitCodeSubscription { get; set; } = null!;
        public decimal LimitMaxValue { get; set; }
    }
}