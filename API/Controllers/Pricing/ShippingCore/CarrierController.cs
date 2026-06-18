using Application.DTOs.ShippingCore;
using Application.Interfaces.Services.Pricing.ShippingCore;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.Pricing.ShippingCore
{
    [ApiController]
    [Route("api/carriers")]
    [EnableRateLimiting("ReadPolicy")]
    [Authorize]
    public class CarrierController : ControllerBase
    {
        private readonly ICarrierService _carrierService;

        public CarrierController(ICarrierService carrierService)
        {
            _carrierService = carrierService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllCarriers([FromQuery] QueryParameters query)
        {
            var carriers = await _carrierService.GetAllAsync(query);

            if (carriers == null || !carriers.Any())
                return NotFound(new { message = "No carriers found" });

            return Ok(carriers);
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCarrierById(Guid id)
        {
            var carrier = await _carrierService.GetByIdAsync(id);

            if (carrier == null)
                return NotFound(new { message = "Carrier not found" });

            return Ok(carrier);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateCarrier([FromBody] CreateCarrierRequest request)
        {
            var result = await _carrierService.CreateAsync(request, getCurrentUser());

            return CreatedAtAction(nameof(GetCarrierById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateCarrier(Guid id, [FromBody] UpdateCarrierRequest request)
        {
            var updated = await _carrierService.UpdateAsync(id, request, getCurrentUser());

            if (updated == null)
                return NotFound(new { message = "Carrier not found" });

            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCarrier(Guid id)
        {
            await _carrierService.DeleteAsync(id, getCurrentUser());

            return Ok(new { message = "Carrier deleted successfully" });
        }

        private string getCurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("user not found");
    }
}