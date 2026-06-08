using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Shipments
{
    public class ShipmentCharge
    {
        public Guid Id { get; set; }

        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;

        public ICollection<ShipmentChargeItem> ChargeItems { get; set; } = new HashSet<ShipmentChargeItem>();

        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public string Currency { get; set; } = "EGP";

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
    public class ShipmentChargeItem
    {
        public Guid ShipmentChargeId { get; set; }
        public ShipmentCharge ShipmentCharge { get; set; } = null!;

        public Guid ShipmentItemId { get; set; }
        public ShipmentItem ShipmentItem { get; set; } = null!;
    }

}