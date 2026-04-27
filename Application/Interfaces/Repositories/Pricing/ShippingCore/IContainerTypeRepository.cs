using Application.Interfaces.Repositories.Patterns;
using Application.Models;
using Domain.Entities.ShippingCore;

namespace Application.Interfaces.Repositories.ShippingCore
{
    public interface IContainerTypeRepository : IGenericRepository<ContainerType>
    {
        Task<ContainerType?> GetByNameAsync(string input);
        Task<IEnumerable<ContainerType>> GetAllAsync(QueryParameters query);
    }
}