using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Enums;

namespace Domain.Entities.Payments
{
    public class PaymentTransaction
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = null!;

        public Guid? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        public Guid? UserSubscriptionId { get; set; }
        public UserSubscription? UserSubscription { get; set; }

        public Guid? SubscriptionPlanId { get; set; }
        public SubscriptionPlan? SubscriptionPlan { get; set; }


        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";

        public PaymentProvider Provider { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentTransactionStatus Status { get; set; }

        public string? ProviderOrderId { get; set; }
        public string? ClientSecret { get; set; }
        public string? ProviderIntentionId { get; set; }
        public string? ProviderTransactionId { get; set; }

        public string? GatewayResponse { get; set; }
        public string? FailureReason { get; set; }

        public DateTimeOffset? PaidAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}