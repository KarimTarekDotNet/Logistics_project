namespace Domain.Entities.Shipments
{
    public class ShipmentItem
    {
        public Guid Id { get; set; }

        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;

        public string Description { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Weight { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
    }
}