using Domain.Enums;

namespace Domain.Entities.Shipments
{
    public class InvoicePayment
    {
        public Guid Id { get; set; }

        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "USD";

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentProvider PaymentProvider { get; set; }

        public PaymentTransactionStatus Status { get; set; }

        public string? TransactionId { get; set; }

        public string? ReferenceNumber { get; set; }

        public DateTimeOffset PaidAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}