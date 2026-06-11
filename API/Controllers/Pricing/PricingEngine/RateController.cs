using Application.DTOs.Pricing.PricingEngine.Rates;
using Application.DTOs.Pricing.Recommendations;
using Application.Interfaces.Services.Pricing.PricingEngine;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
            return Ok(await _rateService.CountAsync());
        }

        [HttpGet]
        public async Task<IActionResult> SearchAsync([FromQuery] RateParameters query)
        {
            var rates = await _rateService.SearchAsync(query);

            return Ok(rates);
        }

        [HttpGet("market-analytics")]
        public async Task<IActionResult> GetMarketAnalyticsAsync([FromQuery] QueryMarketRequest request)
        {
            var rates = await _rateService.GetMarketAnalyticsAsync
            (request.RouteId, request.ContainerId, request.Currency);

            return Ok(rates);
        }

        [HttpPost("recommended")]
        public async Task<IActionResult> RecommendationAsync([FromQuery] RateRecommendationRequest request)
        {
            var recommended = await _rateService.RecommendationAsync(request);
            if (recommended == null)
                return NotFound(new { message = "No recommended rates found." });

            return Ok(recommended);
        }

        // GET: api/rates/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRateById(Guid id)
        {
            var rate = await _rateService.GetByIdAsync(id);

            return Ok(rate);
        }

        // POST: api/rates
        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateRate([FromBody] CreateRateRequest request)
        {
            var result = await _rateService.CreateAsync(request);

            return CreatedAtAction(nameof(GetRateById),
                new { id = result.Id },
                result);
        }

        // PUT: api/rates/{id}
        [HttpPut("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateRate(Guid id, [FromBody] UpdateRateRequest request)
        {
            var updated = await _rateService.UpdateAsync(id, request);

            return Ok(updated);
        }

        // DELETE: api/rates/{id}
        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRate(Guid id)
        {
            await _rateService.DeleteAsync(id);

            return Ok(new { message = "Rate deleted successfully" });
        }

        // PATCH: api/rates/{id}/active
        [HttpPatch("{id:guid}/active")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRateActive(Guid id)
        {
            var result = await _rateService.ChangeRateActive(id);

            return Ok(new
            {
                message = result ? "Rate activated" : "Rate deactivated"
            });
        }
    }
}