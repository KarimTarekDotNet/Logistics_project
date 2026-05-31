using Domain.Enums;

namespace Application.DTOs.Shipments.Core
{
    public class CreateInvoicePaymentRequest
    {
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "USD";

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentProvider PaymentProvider { get; set; }

        public PaymentTransactionStatus Status { get; set; }

        public string? TransactionId { get; set; }

        public string? ReferenceNumber { get; set; }
    }

    public record InvoicePaymentResponse
    {
        public Guid Id { get; set; }
        public string TransactionId { get; set; } = null!;
        public string ReferenceNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTimeOffset PaidAt { get; set; }
    }
}
