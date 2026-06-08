namespace Domain.Entities.Shipments
{
    public class ShipmentItem
    {
        public Guid Id { get; set; }

        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;

        public ICollection<ShipmentChargeItem> ChargeItems { get; set; } = new HashSet<ShipmentChargeItem>();

        public string Description { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal ChargeableWeight { get; set; }

        public decimal GrossWeight { get; set; }
        public decimal NetWeight { get; set; }
        public decimal VolumeCbm { get; set; }
        public bool IsHazardous { get; set; }
        public decimal? RequiredTemperatureCelsius { get; set; }
        public string? MarksAndNumbers { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
    }
}