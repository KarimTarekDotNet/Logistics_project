using Application.Interfaces.Repositories.Patterns;
using Application.Models;
using Domain.Entities.ShippingCore;

namespace Application.Interfaces.Repositories.ShippingCore
{
    public interface IPortRepository : IGenericRepository<Port>
    {
        Task<Port?> GetByNameOrCodeAsync(string input);
        Task<Port?> GetByCodeAsync(string code);
        Task<IEnumerable<Port>> GetAllAsync(QueryParameters query);
        Task<IEnumerable<Port>> GetByCountryAsync(string country, QueryParameters query);
    }
}