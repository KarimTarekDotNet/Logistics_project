namespace Domain.Entities.Users.Subscriptions
{
    public class SubscriptionFeature
    {
        public Guid Id { get; set; }

        public string FeatureCode { get; set; } = null!;
        public string FeatureName { get; set; } = null!;

        public ICollection<SubscriptionPlanFeature> PlanFeatures { get; set; } = new HashSet<SubscriptionPlanFeature>();
    }
}