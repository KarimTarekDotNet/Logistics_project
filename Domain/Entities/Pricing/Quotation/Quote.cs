using Domain.Entities.Shipments;
using Domain.Entities.ShippingCore;
using Domain.Entities.Users;

namespace Domain.Entities.Pricing.Quotation
{
    public class Quote // Shipping quote provided to customers based on selected route and container type
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public Guid RouteId { get; set; }
        public Route Route { get; set; } = null!;

        public Guid ContainerTypeId { get; set; }
        public ContainerType ContainerType { get; set; } = null!;

        public Shipment? Shipment { get; set; }

        public decimal FinalPrice { get; set; }
        public string Currency { get; set; } = null!;

        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}