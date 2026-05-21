using Domain.Enums;

namespace Application.DTOs.Pricing.Quotation
{
    public record QuoteResponse
    {
        public Guid Id { get; init; }

        public Guid CustomerId { get; init; }
        public string CustomerName { get; init; } = null!;

        public Guid RateId { get; init; }

        public Guid RouteId { get; init; }
        public string FromPortCode { get; init; } = null!;
        public string ToPortCode { get; init; } = null!;

        public Guid CarrierId { get; init; }
        public string CarrierName { get; init; } = null!;

        public Guid ContainerTypeId { get; init; }
        public string ContainerTypeName { get; init; } = null!;

        public decimal FinalPrice { get; init; }
        public string Currency { get; init; } = null!;

        public decimal RequestedGrossWeightKg { get; init; }
        public decimal RequestedNetWeightKg { get; init; }
        public decimal RequestedVolumeCbm { get; init; }

        public bool IsHazardous { get; init; }

        public decimal? RequiredTemperatureCelsius { get; init; }

        public QuoteStatus Status { get; init; }

        public DateTimeOffset CreatedAt { get; init; }
    }

    public record CreateQuoteRequest
    {
        public Guid CustomerId { get; init; }
        public Guid RateId { get; init; }

        public decimal RequestedGrossWeightKg { get; init; }
        public decimal RequestedNetWeightKg { get; init; }
        public decimal RequestedVolumeCbm { get; init; }

        public bool IsHazardous { get; init; }

        public decimal? RequiredTemperatureCelsius { get; init; }
    }
}