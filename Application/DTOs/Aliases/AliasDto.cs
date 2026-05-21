using Domain.Enums;

namespace Application.DTOs.Aliases
{
    public record AliasResolvedResponse
    {
        public bool Resolved { get; set; }

        public Guid? EntityId { get; set; }

        public string? MatchedAlias { get; set; }

        public string Type { get; init; } = null!;
    }
    public record AliasResolvedRequest
    {
        public string Value { get; set; } = null!;
        public AliasType Type { get; set; }
    }
    public record AliasResponse
    {
        public Guid Id { get; init; }

        public string AliasName { get; init; } = null!;

        public string NormalizedAlias { get; init; } = null!;

        public Guid EntityId { get; init; }

        public AliasType Type { get; init; }
    }
    public record CreateAliasRequest
    {
        public string AliasName { get; init; } = null!;

        public Guid EntityId { get; init; }

        public AliasType Type { get; init; }
    }

    public record UpdateAliasRequest
    {
        public string? AliasName { get; set; }

        public Guid? EntityId { get; set; }

        public AliasType? Type { get; set; }
    }
}