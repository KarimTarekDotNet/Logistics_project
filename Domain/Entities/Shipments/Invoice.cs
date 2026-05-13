using Domain.Enums;

namespace Domain.Entities.Shipments
{
    public class Invoice
    {
        public Guid Id { get; set; }

        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;

        public string InvoiceNumber { get; set; } = null!;
        public string Currency { get; set; } = "USD";

        public ICollection<ShipmentCharge> Charges { get; set; } = new List<ShipmentCharge>();

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public DateTimeOffset IssuedAt { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public DateTimeOffset? PaidAt { get; set; }

        public PayerType PayerType { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        public DateTimeOffset? CancelledAt { get; set; }
        public string? CancelledByUserId { get; set; }
        public string? CancellationReason { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
    }
}