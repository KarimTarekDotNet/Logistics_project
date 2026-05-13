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

        public string? ChangedByUserId { get; set; } = "Unknown";
        public string? ChangedByRole { get; set; } = "Unknown";
        public string? ChangedBy { get; set; } = "Unknown";
        public string? Reason { get; set; } = "Unknown";
    }

    public class ShipmentTimelineItemResponse
    {
        public string Type { get; set; } = null!;

        public string Category { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public decimal? Amount { get; set; }

        public string? Currency { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public string? CreatedBy { get; set; }
    }   
}
