using Domain.Enums;

namespace Application.DTOs.Shipments.Core
{
    public record CreateShipmentRequest
    {
        public Guid QuoteId { get; set; }
        public Guid CarrierId { get; set; }
    }
    public record UpdateShipmentRequest
    {
        public Guid? QuoteId { get; set; }
        public Guid? CarrierId { get; set; }
    }

    public record ShipmentResponse
    {
        public Guid Id { get; set; }

        public Guid QuoteId { get; set; }
        public Guid RouteId { get; set; }

        public string CustomerName { get; set; } = null!;
        public string ContainerTypeName { get; set; } = null!;

        public string CarrierName { get; set; } = null!;

        public decimal AgreedPrice { get; set; }
        public string Currency { get; set; } = null!;

        public string Status { get; set; } = null!;

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ClientConfirmedAt { get; set; }
        public DateTimeOffset? BookingRequestedAt { get; set; }
        public DateTimeOffset? BookingConfirmedAt { get; set; }
        public DateTimeOffset? DeliveredAt { get; set; }

        public ICollection<ShipmentItemResponse> Items { get; set; } = new List<ShipmentItemResponse>();
        public ICollection<ShipmentChargeResponse> Charges { get; set; } = new List<ShipmentChargeResponse>();
        public ICollection<ShipmentStatusHistoryResponse> StatusHistory { get; set; } = new List<ShipmentStatusHistoryResponse>();
    }

    public record ChangeShipmentStatusRequest
    {
        public ShipmentStatus ToStatus { get; set; }
        public string? Reason { get; set; }
    }
}
