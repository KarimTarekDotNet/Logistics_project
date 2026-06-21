using Application.Common;
using Application.DTOs.Pricing.PricingEngine.Rates;
using Application.DTOs.Pricing.Recommendations;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.PricingEngine
{
    public interface IRateService
    {
        Task<Result<RateResponse>> GetByIdAsync(Guid id);
        Task<Result<RateResponse>> CreateAsync(CreateRateRequest dto, string userId);
        Task<Result<RateResponse>> UpdateAsync(Guid id, UpdateRateRequest dto, string userId);
        Task<Result<IEnumerable<RateResponse>>> SearchAsync(RateParameters query);
        Task<Result<MarketAnalyticsResponse>> GetMarketAnalyticsAsync(Guid routeId, Guid containerId, string currency);
        Task<Result<RateRecommendationResponse>> RecommendationAsync(RateRecommendationRequest dto);
        Task<Result> DeleteAsync(Guid id, string userId);
        Task<Result<bool>> ChangeRateActive(Guid rateId, string userId);
        Task<Result<int>> CountAsync();
    }
}
