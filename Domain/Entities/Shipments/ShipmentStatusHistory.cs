using Domain.Enums;

namespace Domain.Entities.Shipments
{
    public class ShipmentStatusHistory
    {
        public Guid Id { get; set; }

        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;

        public ShipmentStatus FromStatus { get; set; }
        public ShipmentStatus ToStatus { get; set; }

        public DateTimeOffset ChangedAt { get; set; }

        public string? ChangedBy { get; set; }
        public string? Reason { get; set; }
    }
}