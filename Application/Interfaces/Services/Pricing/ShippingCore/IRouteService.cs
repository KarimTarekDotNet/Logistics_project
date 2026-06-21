using Application.Common;
using Application.DTOs.ShippingCore;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.ShippingCore
{
    public interface IRouteService
    {
        Task<Result<RouteResponse>> GetByIdAsync(Guid id);
        Task<Result<IEnumerable<RouteResponse>>> GetAllAsync(QueryParameters query);
        Task<Result<IEnumerable<RouteResponse>>> GetByFromPortAsync(Guid fromPortId, QueryParameters query);
        Task<Result<IEnumerable<RouteResponse>>> GetByToPortAsync(Guid toPortId, QueryParameters query);
        Task<Result<RouteResponse>> CreateAsync(CreateRouteRequest dto, string userId);
        Task<Result<RouteResponse>> UpdateAsync(Guid id, UpdateRouteRequest dto, string userId);
        Task<Result> DeleteAsync(Guid id, string userId);
    }
}
