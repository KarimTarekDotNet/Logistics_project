using API.Extensions;
using Application.DTOs.Pricing.PricingEngine.Rates;
using Application.DTOs.Pricing.Recommendations;
using Application.Interfaces.Services.Pricing.PricingEngine;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.Pricing.PricingEngine
{
    [ApiController]
    [Route("api/rates")]
    [EnableRateLimiting("ReadPolicy")]
    [Authorize]
    public class RateController : ControllerBase
    {
        private readonly IRateService _rateService;

        public RateController(IRateService rateService)
        {
            _rateService = rateService;
        }

        [HttpGet("count")]
        [AllowAnonymous]
        public async Task<IActionResult> Count()
        {
            var result = await _rateService.CountAsync();
            return result.ToActionResult(this);
        }

        [HttpGet]
        public async Task<IActionResult> SearchAsync([FromQuery] RateParameters query)
        {
            var result = await _rateService.SearchAsync(query);
            return result.ToActionResult(this);
        }

        [HttpGet("market-analytics")]
        public async Task<IActionResult> GetMarketAnalyticsAsync([FromQuery] QueryMarketRequest request)
        {
            var result = await _rateService.GetMarketAnalyticsAsync(request.RouteId, request.ContainerId, request.Currency);
            return result.ToActionResult(this);
        }

        [HttpPost("recommended")]
        public async Task<IActionResult> RecommendationAsync([FromQuery] RateRecommendationRequest request)
        {
            var result = await _rateService.RecommendationAsync(request);
            return result.ToActionResult(this);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRateById(Guid id)
        {
            var result = await _rateService.GetByIdAsync(id);
            return result.ToActionResult(this);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateRate([FromBody] CreateRateRequest request)
        {
            var result = await _rateService.CreateAsync(request, getCurrentUser());
            return result.ToCreatedResult(this, nameof(GetRateById), new { id = result.Value?.Id });
        }

        [HttpPut("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateRate(Guid id, [FromBody] UpdateRateRequest request)
        {
            var result = await _rateService.UpdateAsync(id, request, getCurrentUser());
            return result.ToActionResult(this);
        }

        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRate(Guid id)
        {
            var result = await _rateService.DeleteAsync(id, getCurrentUser());
            if (result.IsSuccess) return Ok(new { message = "Rate deleted successfully" });
            return result.ToActionResult(this);
        }

        [HttpPatch("{id:guid}/active")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRateActive(Guid id)
        {
            var result = await _rateService.ChangeRateActive(id, getCurrentUser());
            if (!result.IsSuccess) return result.ToActionResult(this);
            return Ok(new { message = result.Value ? "Rate activated" : "Rate deactivated" });
        }

        private string getCurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not found");
    }
}
