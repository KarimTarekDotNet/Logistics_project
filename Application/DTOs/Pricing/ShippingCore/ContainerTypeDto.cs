namespace Application.DTOs.ShippingCore
{
    public record ContainerTypeResponse(Guid Id, string Name);

    public record CreateContainerTypeRequest(string Name);

    public record UpdateContainerTypeRequest(string Name);
}
