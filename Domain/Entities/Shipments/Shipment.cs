using Domain.Entities.Pricing.Quotation;
using Domain.Entities.ShippingCore;
using Domain.Entities.Users;
using Domain.Enums;

namespace Domain.Entities.Shipments
{
    public class Shipment
    {
        public Guid Id { get; set; }

        public Guid QuoteId { get; set; }
        public Quote Quote { get; set; } = null!;

        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public Guid RouteId { get; set; }
        public Route Route { get; set; } = null!;

        public Guid ContainerTypeId { get; set; }
        public ContainerType ContainerType { get; set; } = null!;

        public Guid CarrierId { get; set; }
        public Carrier Carrier { get; set; } = null!;

        public decimal AgreedPrice { get; set; }
        public string Currency { get; set; } = null!;
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Created;
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

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        public ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
        public ICollection<ShipmentCharge> Charges { get; set; } = new List<ShipmentCharge>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<ShipmentStatusHistory> StatusHistory { get; set; } = new List<ShipmentStatusHistory>();
        public ICollection<ShipmentDocument> ShipmentDocuments { get; set; } = new List<ShipmentDocument>();
    }
}