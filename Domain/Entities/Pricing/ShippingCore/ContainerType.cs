namespace Domain.Entities.ShippingCore
{
    public class ContainerType // Type of shipping container (e.g., 20ft, 40ft)
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
