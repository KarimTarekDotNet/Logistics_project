using Application.DTOs.ShippingCore;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.ShippingCore
{
    public interface ICarrierService
    {
        Task<CarrierResponse?> GetByIdAsync(Guid id);
        Task<IEnumerable<CarrierResponse>> GetAllAsync(QueryParameters query);
        Task<CarrierResponse?> GetByNameOrCodeAsync(string input);
        Task<CarrierResponse> CreateAsync(CreateCarrierRequest dto);
        Task<CarrierResponse> UpdateAsync(Guid id, UpdateCarrierRequest dto);
        Task DeleteAsync(Guid id);
    }
}