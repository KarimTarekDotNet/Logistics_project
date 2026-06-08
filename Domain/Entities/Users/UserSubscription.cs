using Domain.Entities.Payments;

namespace Domain.Entities.Users
{
    public class UserSubscription
    {
        public Guid Id { get; set; }

        public string UserId { get; set; } = null!;

        public Guid SubscriptionPlanId { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; } = null!;

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public bool IsActive { get; set; }

        public ICollection<PaymentTransaction> Payments { get; set; } = new HashSet<PaymentTransaction>();

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
    }
}