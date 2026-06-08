namespace Application.DTOs.User
{
    public record SubscriptionPlanResponse
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = null!;
        public string Description { get; init; } = null!;
        public decimal Price { get; init; }
        public int DurationInDays { get; init; }
        public bool IsActive { get; init; }
    }

    public record CreateSubscriptionPlanRequest
    {
        public string Title { get; init; } = null!;
        public string Description { get; init; } = null!;
        public decimal Price { get; init; }
        public int DurationInDays { get; init; }
    }
    public record UpdateSubscriptionPlanRequest
    {
        public string? Title { get; init; }
        public string? Description { get; init; }
        public decimal? Price { get; init; }
        public int? DurationInDays { get; init; }
    }
}
