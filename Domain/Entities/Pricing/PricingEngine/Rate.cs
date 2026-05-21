using Domain.Entities.ShippingCore;

namespace Domain.Entities.Pricing.PricingEngine
{
    public class Rate // Shipping rate for a specific carrier, route, and container type
    {
        public Guid Id { get; set; }

        public Guid CarrierId { get; set; }
        public Carrier Carrier { get; set; } = null!;

        public Guid RouteId { get; set; }
        public Route Route { get; set; } = null!;

        public Guid ContainerTypeId { get; set; }
        public ContainerType ContainerType { get; set; } = null!;

        public decimal Price { get; set; }
        public string Currency { get; set; } = null!;

        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset ValidTo { get; set; }

        public decimal? MaxGrossWeightKg { get; set; }
        public decimal? MaxNetWeightKg { get; set; }
        public decimal? MaxVolumeCbm { get; set; }
        public bool AllowsHazardous { get; set; }
        public decimal? MinTemperatureCelsius { get; set; }
        public decimal? MaxTemperatureCelsius { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public bool IsActive { get; set; } = false;

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}