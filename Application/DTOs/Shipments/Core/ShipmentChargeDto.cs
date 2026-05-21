using Domain.Enums;

namespace Application.DTOs.Shipments.Core
{
    public record GenerateShipmentChargesRequest
    {
        public Guid ShipmentId { get; init; }
        public ChargeType ChargeType { get; init; }
        public PayerType PayerType { get; init; }
    }

    public record UpdateShipmentChargeRequest
    {
        public ChargeType? ChargeType { get; init; }
        public PayerType? PayerType { get; init; }
        public decimal? Amount { get; init; }
        public decimal? TaxAmount { get; init; }
        public string? Currency { get; init; }
        public string? Description { get; init; }
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
