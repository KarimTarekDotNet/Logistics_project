using Domain.Enums;

namespace Application.DTOs.Payment
{
    public record StartPaymentRequest
    {
        public Guid InvoiceId { get; set; }
    }

    public record StartPaymentResponse
    {
        public Guid PaymentTransactionId { get; set; }
        public string ClientSecret { get; set; } = null!;
        public PaymentTransactionStatus Status { get; set; }
    }

    public record PaymentTransactionResponse
    {
        public Guid Id { get; set; }
        public Guid? InvoiceId { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;

        public string Provider { get; set; } = null!;
        public string Method { get; set; } = null!;
        public string Status { get; set; } = null!;

        public string? FailureReason { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
