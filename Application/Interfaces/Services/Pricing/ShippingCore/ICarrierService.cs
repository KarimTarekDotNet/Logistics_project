using Application.Common;
using Application.DTOs.ShippingCore;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.ShippingCore
{
    public interface ICarrierService
    {
        Task<Result<CarrierResponse>> GetByIdAsync(Guid id);
        Task<Result<IEnumerable<CarrierResponse>>> GetAllAsync(QueryParameters query);
        Task<Result<CarrierResponse>> GetByNameOrCodeAsync(string input);
        Task<Result<CarrierResponse>> CreateAsync(CreateCarrierRequest dto, string userId);
        Task<Result<CarrierResponse>> UpdateAsync(Guid id, UpdateCarrierRequest dto, string userId);
        Task<Result> DeleteAsync(Guid id, string userId);
    }
}
