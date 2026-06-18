namespace Domain.Entities.Users.Subscriptions
{
    public class SubscriptionFeature
    {
        public Guid Id { get; set; }

        public string FeatureName { get; set; } = null!;
        public string FeatureCode { get; set; } = null!;

        public ICollection<SubscriptionPlanLimit> PlanLimits { get; set; } = new HashSet<SubscriptionPlanLimit>();
        public ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new HashSet<SubscriptionPlan>();
    }
}