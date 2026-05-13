namespace Application.DTOs.Shipments.Core
{
    public record CreateShipmentRequest
    {
        public Guid QuoteId { get; set; }
    }
    public record UpdateShipmentRequest
    {
        public string? BookingNumber { get; set; }
        public string? VesselName { get; set; }
        public string? VoyageNumber { get; set; }
        public string? CurrentCheckpoint { get; set; }

        public DateTimeOffset? EstimatedDeparture { get; set; }
        public DateTimeOffset? EstimatedArrival { get; set; }
        public DateTimeOffset? ActualDeparture { get; set; }
        public DateTimeOffset? ActualArrival { get; set; }
    }

    public record UpdateShipmentTrackingRequest
    {
        public string? BookingNumber { get; set; }
        public string? VesselName { get; set; }
        public string? VoyageNumber { get; set; }
        public string? CurrentCheckpoint { get; set; }

        public DateTimeOffset? EstimatedDeparture { get; set; }
        public DateTimeOffset? EstimatedArrival { get; set; }
        public DateTimeOffset? ActualDeparture { get; set; }
        public DateTimeOffset? ActualArrival { get; set; }
    }

    public record ShipmentResponse
    {
        public Guid Id { get; set; }

        public Guid QuoteId { get; set; }
        public Guid RouteId { get; set; }
        public Guid CarrierId { get; set; }
        public Guid ContainerTypeId { get; set; }
        public Guid CustomerId { get; set; }

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
        public DateTimeOffset? ShippingInstructionsSubmittedAt { get; set; }
        public DateTimeOffset? DraftBlReceivedAt { get; set; }
        public DateTimeOffset? DraftBlApprovedAt { get; set; }
        public DateTimeOffset? PaymentPendingAt { get; set; }
        public DateTimeOffset? PaymentConfirmedAt { get; set; }
        public DateTimeOffset? TelexReleasedAt { get; set; }
        public DateTimeOffset? DeliveredAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public string? BookingNumber { get; set; }
        public string? VesselName { get; set; }
        public string? VoyageNumber { get; set; }
        public string? CancellationReason { get; set; }
        public string? HoldReason { get; set; }
        public string? CurrentCheckpoint { get; set; }
        public DateTimeOffset? EstimatedDeparture { get; set; }
        public DateTimeOffset? EstimatedArrival { get; set; }
        public DateTimeOffset? ActualDeparture { get; set; }
        public DateTimeOffset? ActualArrival { get; set; }


        public ICollection<ShipmentItemResponse> Items { get; set; } = new List<ShipmentItemResponse>();
        public ICollection<ShipmentChargeResponse> Charges { get; set; } = new List<ShipmentChargeResponse>();
        public ICollection<ShipmentStatusHistoryResponse> StatusHistory { get; set; } = new List<ShipmentStatusHistoryResponse>();
    }

    public record ChangeShipmentStatusRequest
    {
        public string? Reason { get; set; }
    }
}
