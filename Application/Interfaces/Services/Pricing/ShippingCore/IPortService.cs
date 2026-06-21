using Application.Common;
using Application.DTOs.ShippingCore;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.ShippingCore
{
    public interface IPortService
    {
        Task<Result<PortResponse>> GetByIdAsync(Guid id);
        Task<Result<IEnumerable<PortResponse>>> GetAllAsync(QueryParameters query);
        Task<Result<IEnumerable<PortResponse>>> GetByCountryAsync(string country, QueryParameters query);
        Task<Result<PortResponse>> CreateAsync(CreatePortRequest dto, string userId);
        Task<Result<PortResponse>> UpdateAsync(Guid id, UpdatePortRequest dto, string userId);
        Task<Result> DeleteAsync(Guid id, string userId);
    }
}
