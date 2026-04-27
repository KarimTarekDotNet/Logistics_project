using Application.Interfaces.Repositories.Patterns;
using Application.Models;
using Domain.Entities.ShippingCore;

namespace Application.Interfaces.Repositories.ShippingCore
{
    public interface IRouteRepository : IGenericRepository<Route>
    {
        Task<Route?> GetWithPortsAsync(Guid id);
        Task<Route?> GetByPortsAsync(Guid fromPortId, Guid toPortId);
        Task<IEnumerable<Route>> GetByFromPortAsync(Guid fromPortId, QueryParameters query);
        Task<IEnumerable<Route>> GetByToPortAsync(Guid toPortId, QueryParameters query);
        Task<IEnumerable<Route>> GetAllAsync(QueryParameters query);
    }
}