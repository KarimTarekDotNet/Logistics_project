using Application.DTOs.Pricing.PricingEngine.Rates;
using Application.DTOs.Pricing.Recommendations;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.PricingEngine
{
    public interface IRateService
    {
        Task<RateResponse?> GetByIdAsync(Guid id);
        Task<RateResponse> CreateAsync(CreateRateRequest dto);
        Task<RateResponse> UpdateAsync(Guid id, UpdateRateRequest dto);
        Task<IEnumerable<RateResponse>> SearchAsync(RateParameters query);
        Task<MarketAnalyticsResponse> GetMarketAnalyticsAsync(Guid routeId, Guid containerId, string currency);
        Task<RateRecommendationResponse> RecommendationAsync(RateRecommendationRequest dto);
        Task DeleteAsync(Guid id);
        Task<bool> ChangeRateActive(Guid rateId);
        Task<int> CountAsync();
    }
}