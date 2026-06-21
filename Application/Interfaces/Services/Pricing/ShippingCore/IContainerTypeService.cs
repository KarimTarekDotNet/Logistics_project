using Application.Common;
using Application.DTOs.ShippingCore;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.ShippingCore
{
    public interface IContainerTypeService
    {
        Task<Result<ContainerTypeResponse>> GetByIdAsync(Guid id);
        Task<Result<IEnumerable<ContainerTypeResponse>>> GetAllAsync(QueryParameters query);
        Task<Result<ContainerTypeResponse>> CreateAsync(CreateContainerTypeRequest dto, string userId);
        Task<Result<ContainerTypeResponse>> UpdateAsync(Guid id, UpdateContainerTypeRequest dto, string userId);
        Task<Result> DeleteAsync(Guid id, string userId);
    }
}
