namespace Application.DTOs.Pricing.PricingEngine
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
        public DateTimeOffset CreatedAt { get; init; }
        public bool IsActive { get; init; }
    }

    public record CreateRateRequest(
        Guid CarrierId,
        Guid RouteId,
        Guid ContainerTypeId,
        decimal Price,
        string Currency,
        DateTimeOffset ValidFrom,
        DateTimeOffset ValidTo);

    public record UpdateRateRequest
    {
        public decimal Price { get; set; }
        public string? Currency { get; set; }
        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset ValidTo { get; set; }
    }
}
