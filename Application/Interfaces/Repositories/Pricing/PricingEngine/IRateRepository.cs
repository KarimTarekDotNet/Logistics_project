using Application.Interfaces.Repositories.Patterns;
using Application.Models;
using Domain.Entities.Pricing.PricingEngine;

namespace Application.Interfaces.Repositories.Pricing.PricingEngine
{
    public interface IRateRepository : IGenericRepository<Rate>
    {
        Task<Rate?> GetById(Guid Id);
        Task<Rate?> GetByIdWithDetailsAsync(Guid Id);
        Task<IEnumerable<Rate>> GetAvailableRatesByCarrierRouteAndContainerTypeAsync(Guid carrierId, Guid routeId, Guid containerTypeId, 
        DateTimeOffset validFrom, DateTimeOffset validTo);
        IQueryable<Rate> GetRatesByRouteAndContainerTypeQuery(Guid routeId, Guid containerTypeId, string Currency);
        IQueryable<Rate> GetRatesByRouteAndContainerTypeQueryForRecommendation(Guid routeId, Guid containerTypeId, string currency, decimal? maxPrice);
        Task<IEnumerable<Rate>> SearchAsync(RateParameters query);
        Task<IEnumerable<Rate>> GetByCarrierRouteAndContainerTypeAsync(Guid carrierId, Guid routeId, Guid containerTypeId);
        Task<bool> ExistsActiveRateAsync(Guid carrierId, Guid routeId, Guid containerTypeId);
        Task<int?> CountAsync();
    }
}