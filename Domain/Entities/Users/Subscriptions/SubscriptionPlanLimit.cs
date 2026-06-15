namespace Domain.Entities.Users.Subscriptions
{
    public class SubscriptionPlanLimit
    {
        public Guid Id { get; set; }

        public Guid SubscriptionPlanId { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; } = null!;

        public string LimitCodeSubscription { get; set; } = null!;
        public decimal LimitMaxValue { get; set; }
    }
}