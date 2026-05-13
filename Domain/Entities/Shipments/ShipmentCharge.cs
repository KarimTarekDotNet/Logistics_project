using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Shipments
{
    public class ShipmentCharge
    {
        public Guid Id { get; set; }

        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;

        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public string Currency { get; set; } = "USD";

        [NotMapped]
        public decimal TotalAmount => Amount + TaxAmount;

        public ChargeType ChargeType { get; set; } = ChargeType.Other;
        public PayerType PayerType { get; set; }

        public Guid? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
    }
}