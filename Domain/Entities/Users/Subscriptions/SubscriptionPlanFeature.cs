namespace Domain.Entities.Users.Subscriptions
{
    public class SubscriptionPlanFeature
    {
        public Guid Id { get; set; }
        public Guid SubscriptionPlanId { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; } = null!;

        public Guid SubscriptionFeatureId { get; set; }
        public SubscriptionFeature SubscriptionFeature { get; set; } = null!;
    }
}