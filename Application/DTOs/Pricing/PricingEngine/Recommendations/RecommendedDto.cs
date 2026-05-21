using Application.DTOs.Pricing.PricingEngine.Rates;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Pricing.Recommendations
{
    public record RateRecommendationResponse
    {
        public List<RecommendedRateResponse> Recommendations { get; set; } = [];
    }

    public record RecommendedRateResponse
    {
        public RateResponse Rate { get; set; } = null!;

        public int Score { get; set; }

        public string RecommendationReason { get; set; } = null!;

        public int? TransitDays { get; set; }

        public MarketPosition MarketPosition { get; set; }
        
        public bool IsCheapest { get; set; }
    }

    public record RateRecommendationRequest
    {
        [Required]
        public Guid RouteId { get; set; }

        [Required]
        public Guid ContainerTypeId { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "USD";

        [Range(0.01, double.MaxValue)]
        public decimal? MaxPrice { get; set; }

        [Range(1, 20)]
        public int Limit { get; set; } = 5;

        [EnumDataType(typeof(RecommendationPriority))]
        public RecommendationPriority Priority { get; set; }
    }
}