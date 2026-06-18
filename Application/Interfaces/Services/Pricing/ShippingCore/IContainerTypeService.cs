using Application.DTOs.ShippingCore;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.ShippingCore
{
    public interface IContainerTypeService
    {
        Task<ContainerTypeResponse?> GetByIdAsync(Guid id);
        Task<IEnumerable<ContainerTypeResponse>> GetAllAsync(QueryParameters query);
        Task<ContainerTypeResponse> CreateAsync(CreateContainerTypeRequest dto, string userId);
        Task<ContainerTypeResponse> UpdateAsync(Guid id, UpdateContainerTypeRequest dto, string userId);
        Task DeleteAsync(Guid id, string userId);
    }
}