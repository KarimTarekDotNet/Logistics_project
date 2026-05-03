namespace Application.DTOs.Shipments.Core
{
    public record CreateShipmentStatusHistoryRequest
    {
        public Guid ShipmentId { get; set; }
        public string FromStatus { get; set; } = null!;
        public string ToStatus { get; set; } = null!;
        public string? ChangedBy { get; set; }
        public string? Reason { get; set; }
    }

    public record UpdateShipmentStatusHistoryRequest
    {
        public Guid? ShipmentId { get; set; }
        public string? FromStatus { get; set; }
        public string? ToStatus { get; set; }
        public string? ChangedBy { get; set; }
        public string? Reason { get; set; }
    }

    public record ShipmentStatusHistoryResponse
    {
        public Guid Id { get; set; }

        public Guid ShipmentId { get; set; }

        public string FromStatus { get; set; } = null!;
        public string ToStatus { get; set; } = null!;

        public DateTimeOffset ChangedAt { get; set; }

        public string? ChangedBy { get; set; } = "Unknown";
        public string? Reason { get; set; } = "Unknown";
    }
}
