namespace Application.DTOs.ShippingCore
{
    public record CarrierResponse(Guid Id, string Name, string Code);

    public record CreateCarrierRequest(string Name, string Code);

    public record UpdateCarrierRequest
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
    }
}
