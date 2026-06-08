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
    }
}