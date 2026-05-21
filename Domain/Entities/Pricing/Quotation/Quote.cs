using Domain.Entities.Shipments;
using Domain.Entities.ShippingCore;
using Domain.Entities.Users;
using Domain.Enums;

namespace Domain.Entities.Pricing.Quotation
{
    public class Quote // Shipping quote provided to customers based on selected route and container type
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public Guid CarrierId { get; set; }
        public Carrier Carrier { get; set; } = null!;

        public Guid RouteId { get; set; }
        public Route Route { get; set; } = null!;

        public Guid ContainerTypeId { get; set; }
        public ContainerType ContainerType { get; set; } = null!;

        public Guid RateId { get; set; }

        public Shipment? Shipment { get; set; }

        public decimal FinalPrice { get; set; }
        public string Currency { get; set; } = null!;

        // Snapshot of approved cargo request
        public decimal RequestedGrossWeightKg { get; set; }
        public decimal RequestedNetWeightKg { get; set; }
        public decimal RequestedVolumeCbm { get; set; }
        public decimal RequestedChargeableWeightKg { get; set; }

        public bool IsHazardous { get; set; }

        public decimal? RequiredTemperatureCelsius { get; set; }

        public QuoteStatus Status { get; set; }
        public string? Reason { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}