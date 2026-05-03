using Application.Interfaces.Repositories.Patterns;
using Application.Models;
using Domain.Entities.Pricing.PricingEngine;

namespace Application.Interfaces.Repositories.Pricing.PricingEngine
{
    public interface IRateRepository : IGenericRepository<Rate>
    {
        Task<IEnumerable<Rate>> GetAvailableRatesByCarrierRouteAndContainerTypeAsync(Guid carrierId, Guid routeId, Guid containerTypeId);
        Task<IEnumerable<Rate>> SearchAsync(RateParameters query);
        Task<IEnumerable<Rate>> GetByCarrierRouteAndContainerTypeAsync(Guid carrierId, Guid routeId, Guid containerTypeId);
        Task<bool> ExistsActiveRateAsync(Guid carrierId, Guid routeId, Guid containerTypeId);
    }
}