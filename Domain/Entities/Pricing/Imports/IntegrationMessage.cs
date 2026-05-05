using Domain.Enums;

namespace Domain.Entities.Pricing.Imports
{
    public class IntegrationMessage
    {
        public Guid Id { get; set; }
        public string ExternalMessageId { get; set; } = null!;
        public ExternalSource Source { get; set; }
        public Status Status { get; set; } = Status.Pending;
        public DateTimeOffset CreatedAt { get; set; }
    }
}