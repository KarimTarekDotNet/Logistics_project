namespace Application.DTOs.User
{
    public record UserSubscriptionResponse
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = null!;

        public string SubscriptionPlanTitle { get; set; } = null!;

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<UserSubscriptionUsageResponse> Usages { get; set; } = null!;
    }

    public record UserSubscriptionUsageResponse
    {
        public Guid Id { get; set; }

        public string LimitCode { get; set; } = null!;
        public decimal UsedValue { get; set; }
        public DateTimeOffset PeriodStart { get; set; }
        public DateTimeOffset PeriodEnd { get; set; }
    }
}