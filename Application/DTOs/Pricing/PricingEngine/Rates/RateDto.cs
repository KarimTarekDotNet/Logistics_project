namespace Application.DTOs.Pricing.PricingEngine.Rates
{
    public record RateResponse
    {
        public Guid Id { get; init; }

        public Guid CarrierId { get; init; }
        public string CarrierName { get; init; } = null!;

        public Guid RouteId { get; init; }
        public string FromPortCode { get; init; } = null!;
        public string ToPortCode { get; init; } = null!;

        public Guid ContainerTypeId { get; init; }
        public string ContainerTypeName { get; init; } = null!;

        public decimal Price { get; init; }
        public string Currency { get; init; } = null!;

        public DateTimeOffset ValidFrom { get; init; }
        public DateTimeOffset ValidTo { get; init; }

        public decimal? MaxGrossWeightKg { get; init; }
        public decimal? MaxNetWeightKg { get; init; }
        public decimal? MaxVolumeCbm { get; init; }

        public bool? AllowsHazardous { get; init; }

        public decimal? MinTemperatureCelsius { get; init; }
        public decimal? MaxTemperatureCelsius { get; init; }

        public DateTimeOffset CreatedAt { get; init; }
        public bool IsActive { get; init; }
    }

    public record MarketAnalyticsResponse
    {
        public decimal CheapestPrice { get; set; }
        public decimal HighestPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public int ActiveCount { get; set; }

        public string Currency { get; set; } = null!;
    }

    public record CreateRateRequest
    {
        public Guid CarrierId { get; init; }
        public Guid RouteId { get; init; }
        public Guid ContainerTypeId { get; init; }

        public decimal Price { get; init; }
        public string Currency { get; init; } = null!;

        public DateTimeOffset ValidFrom { get; init; }
        public DateTimeOffset ValidTo { get; init; }

        public decimal? MaxGrossWeightKg { get; init; }
        public decimal? MaxNetWeightKg { get; init; }
        public decimal? MaxVolumeCbm { get; init; }

        public bool? AllowsHazardous { get; init; }

        public decimal? MinTemperatureCelsius { get; init; }
        public decimal? MaxTemperatureCelsius { get; init; }
    }

    public record UpdateRateRequest
    {
        public decimal Price { get; set; }
        public string? Currency { get; set; }

        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset ValidTo { get; set; }

        public decimal? MaxGrossWeightKg { get; set; }
        public decimal? MaxNetWeightKg { get; set; }
        public decimal? MaxVolumeCbm { get; set; }

        public bool AllowsHazardous { get; set; }

        public decimal? MinTemperatureCelsius { get; set; }
        public decimal? MaxTemperatureCelsius { get; set; }
    }

    public record QueryMarketRequest(Guid RouteId, Guid ContainerId, string Currency);
}