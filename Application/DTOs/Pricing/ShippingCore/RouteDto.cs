namespace Application.DTOs.ShippingCore
{
    public record RouteResponse
    {
        public Guid Id { get; init; }
        public Guid FromPortId { get; init; }
        public string FromPortName { get; init; } = null!;
        public string FromPortCode { get; init; } = null!;
        public Guid ToPortId { get; init; }
        public string ToPortName { get; init; } = null!;
        public string ToPortCode { get; init; } = null!;
    }

    public record CreateRouteRequest(Guid FromPortId, Guid ToPortId);
    public record UpdateRouteRequest(Guid FromPortId, Guid ToPortId);
}