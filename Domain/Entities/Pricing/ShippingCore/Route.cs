using Domain.Entities.Pricing.PricingEngine;
using Domain.Entities.Shipments;

namespace Domain.Entities.ShippingCore
{
    public class Route // Shipping route between two ports
    {
        public Guid Id { get; set; }

        public Guid FromPortId { get; set; }
        public Port FromPort { get; set; } = null!;

        public Guid ToPortId { get; set; }
        public Port ToPort { get; set; } = null!;

        public ICollection<Rate> Rates { get; set; } = new List<Rate>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
