namespace Domain.Entities.ShippingCore
{
    public class Port // Shipping port
    {
        public Guid Id { get; set; }
        public string Country { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
