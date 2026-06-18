using Application.DTOs.ShippingCore;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.ShippingCore
{
    public interface IPortService
    {
        Task<PortResponse?> GetByIdAsync(Guid id);
        Task<IEnumerable<PortResponse>> GetAllAsync(QueryParameters query);
        Task<IEnumerable<PortResponse>> GetByCountryAsync(string country, QueryParameters query);
        Task<PortResponse> CreateAsync(CreatePortRequest dto, string userId);
        Task<PortResponse> UpdateAsync(Guid id, UpdatePortRequest dto, string userId);
        Task DeleteAsync(Guid id, string userId);
    }
}