namespace Application.DTOs.User
{
    public record SubscriptionPlanResponse
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = null!;
        public string Description { get; init; } = null!;
        public string Currency { get; init; } = null!;
        public decimal Price { get; init; }
        public int DurationInDays { get; init; }
        public bool IsActive { get; init; }
        public ICollection<SubscriptionFeatureResponse> SubscriptionFeatureResponses { get; set; } = null!;
        public ICollection<SubscriptionPlanLimitResponse> SubscriptionPlanLimitResponses { get; set; } = null!;
    }

    public record SubscriptionFeatureResponse
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
    }

    public record SubscriptionPlanLimitResponse
    {
        public Guid Id { get; init; }
        public string Code { get; set; } = null!;
        public decimal MaxValue { get; set; }
    }

    public record CreateSubscriptionPlanRequest
    {
        public string Title { get; init; } = null!;
        public string Description { get; init; } = null!;
        public string Currency { get; init; } = "EGP";
        public decimal Price { get; init; }
        public int DurationInDays { get; init; }
        public ICollection<CreateSubscriptionFeature> CreateSubscriptionFeatures { get; init; } = null!;
        public ICollection<CreateSubscriptionPlanLimit> CreateSubscriptionPlanLimits { get; init; } = null!;
    }

    public record CreateSubscriptionFeature
    {
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
    }

    public record CreateSubscriptionPlanLimit
    {
        public string Code { get; set; } = null!;
        public decimal MaxValue { get; set; }
    }
}