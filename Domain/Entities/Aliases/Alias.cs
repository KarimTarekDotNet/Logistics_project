using Domain.Enums;

namespace Domain.Entities.Aliases
{
    public class Alias
    {
        public Guid Id { get; set; }
        public AliasType Type { get; set; }
        public string AliasName { get; set; } = null!;
        public string NormalizedAlias { get; set; } = null!;
        public Guid EntityId { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
