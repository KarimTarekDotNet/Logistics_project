using Domain.Entities.Pricing.PricingEngine;
using Domain.Entities.Pricing.Quotation;
using Domain.Entities.Shipments;

namespace Domain.Entities.ShippingCore
{
    public class Carrier // Shipping company
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;

        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();

        public ICollection<Rate> Rates { get; set; } = new List<Rate>();

        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}