using Application.DTOs.Pricing.PricingEngine;
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
    public class RateController : ControllerBase
    {
        private readonly IRateService _rateService;

        public RateController(IRateService rateService)
        {
            _rateService = rateService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SearchAsync([FromQuery] RateParameters query)
        {
            var rates = await _rateService.SearchAsync(query);

            if (rates == null || !rates.Any())
                return NotFound(new { message = "No rates found for the given criteria" });

            return Ok(rates);
        }

        // GET: api/rates/{id}
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRateById(Guid id)
        {
            var rate = await _rateService.GetByIdAsync(id);

            if (rate == null)
                return NotFound(new { message = "Rate not found" });

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