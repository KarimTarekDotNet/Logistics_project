namespace Domain.Entities.Audits
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public string UserId { get; set; } = null!;

        public string Action { get; set; } = null!;
        // Create, Update, Delete

        public string EntityName { get; set; } = null!;
        // Shipment, Customer, Product

        public Guid EntityId { get; set; }

        public string? OldValues { get; set; }
        // JSON

        public string? NewValues { get; set; }
        // JSON

        public DateTimeOffset CreatedAt { get; set; }

        public string? IpAddress { get; set; }
    }
}