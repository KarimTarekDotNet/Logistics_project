using Application.Interfaces.Repositories.Patterns;
using Application.Models;
using Domain.Entities.ShippingCore;

namespace Application.Interfaces.Repositories.ShippingCore
{
    public interface ICarrierRepository : IGenericRepository<Carrier>
    {
        Task<Carrier?> GetByNameOrCodeAsync(string input);
        Task<IEnumerable<Carrier>> GetAllAsync(QueryParameters query);
    }
}