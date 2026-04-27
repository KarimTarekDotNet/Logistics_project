using Application.DTOs.ShippingCore;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.ShippingCore
{
    public interface IRouteService
    {
        Task<RouteResponse?> GetByIdAsync(Guid id);
        Task<IEnumerable<RouteResponse>> GetAllAsync(QueryParameters query);
        Task<IEnumerable<RouteResponse>> GetByFromPortAsync(Guid fromPortId, QueryParameters query);
        Task<IEnumerable<RouteResponse>> GetByToPortAsync(Guid toPortId, QueryParameters query);
        Task<RouteResponse> CreateAsync(CreateRouteRequest dto);
        Task<RouteResponse> UpdateAsync(Guid id, UpdateRouteRequest dto);
        Task DeleteAsync(Guid id);
    }
}