using Domain.Entities.Pricing.PricingEngine;
using Domain.Entities.Users;
using Domain.Enums;

namespace Domain.Entities.Pricing.Quotation
{
    public class QuoteRequest
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public Guid RateId { get; set; }
        public Rate Rate { get; set; } = null!;

        public decimal RequestedRatePrice { get; set; }
        public string Currency { get; set; } = null!;

        public decimal RequestedGrossWeightKg { get; set; }

        public decimal RequestedNetWeightKg { get; set; }

        public decimal RequestedVolumeCbm { get; set; }

        public bool IsHazardous { get; set; }

        public decimal? RequiredTemperatureCelsius { get; set; }

        public string? Notes { get; set; }

        public QuoteRequestStatus Status { get; set; } = QuoteRequestStatus.PendingReview;

        public string? RejectionReason { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? ReviewedAt { get; set; }
        public string? ReviewedByUserId { get; set; }
        public ApplicationUser? ReviewedByUser { get; set; }
    }
}