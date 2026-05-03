namespace Application.DTOs.Shipments.Core
{
    public record CreateShipmentChargeRequest
    {
        public Guid ShipmentId { get; set; }
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
    }

    public record UpdateShipmentChargeRequest
    {
        public string? Description { get; set; }
        public decimal? Amount { get; set; }
    }

    public record ShipmentChargeResponse
    {
        public Guid Id { get; set; }
        public Guid ShipmentId { get; set; }
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
