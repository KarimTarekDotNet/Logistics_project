using Domain.Enums;

namespace Application.DTOs.Shipments.Core
{
    public record CreateShipmentChargeRequest
    {
        public Guid ShipmentId { get; set; }
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public string Currency { get; set; } = null!;
        public ChargeType ChargeType { get; set; }
        public PayerType PayerType { get; set; }
    }

    public record UpdateShipmentChargeRequest
    {
        public string? Description { get; set; }
        public decimal? Amount { get; set; }
        public decimal? TaxAmount { get; set; }
        public string? Currency { get; set; }
        public ChargeType? ChargeType { get; set; }
        public PayerType? PayerType { get; set; }
    }

    public record ShipmentChargeResponse
    {
        public Guid Id { get; set; }
        public Guid ShipmentId { get; set; }
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public string Currency { get; set; } = null!;
        public decimal TotalAmount => Amount + TaxAmount;
        public string ChargeType { get; set; } = null!;
        public string PayerType { get; set; } = null!;

        public DateTimeOffset CreatedAt { get; set; }
    }
}
