namespace Application.DTOs.ShippingCore
{
    public record PortResponse(Guid Id, string Name, string Code, string Country);

    public record CreatePortRequest
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Country { get; set; } = null!;
    }

    public record UpdatePortRequest
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Country { get; set; }
    }
}
