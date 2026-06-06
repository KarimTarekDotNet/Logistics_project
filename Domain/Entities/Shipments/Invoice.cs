using Domain.Entities.Payments;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

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

        public decimal NetShipmentPrice { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();
        public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

        [NotMapped]
        public decimal TotalPaidLocal => Payments
            .Where(x => x.Status == PaymentTransactionStatus.Succeeded)
            .Sum(x => x.Amount);

        [NotMapped]
        public decimal RemainingAmountLocal => Math.Max(0, TotalAmount - TotalPaidLocal);


        [NotMapped]
        public decimal TotalPaidOnline => PaymentTransactions
            .Where(x => x.Status == PaymentTransactionStatus.Succeeded)
            .Sum(x => x.Amount);

        [NotMapped]
        public decimal RemainingAmountOnline => Math.Max(0, TotalAmount - TotalPaidOnline);

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Draft;

        public DateTimeOffset IssuedAt { get; set; }
        public DateTimeOffset DueDate { get; set; }

        public PayerType PayerType { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? PaidAt { get; set; }

        public DateTimeOffset? CancelledAt { get; set; }
        public string? CancelledByUserId { get; set; }
        public string? CancellationReason { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
