namespace Application.DTOs.Shipments.Core
{
    public record CreateShipmentItemRequest
    {
        public Guid ShipmentId { get; set; }
        public string Description { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Weight { get; set; }
    }

    public record UpdateShipmentItemRequest
    {
        public Guid? ShipmentId { get; set; }
        public string? Description { get; set; }
        public int? Quantity { get; set; }
        public decimal? Weight { get; set; }
    }

    public record ShipmentItemResponse
    {
        public Guid Id { get; set; }

        public Guid ShipmentId { get; set; }

        public string Description { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Weight { get; set; }

    }
}
