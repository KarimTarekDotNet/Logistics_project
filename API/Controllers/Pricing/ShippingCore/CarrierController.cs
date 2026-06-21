using API.Extensions;
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
            var result = await _carrierService.GetAllAsync(query);
            return result.ToActionResult(this);
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCarrierById(Guid id)
        {
            var result = await _carrierService.GetByIdAsync(id);
            return result.ToActionResult(this);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateCarrier([FromBody] CreateCarrierRequest request)
        {
            var result = await _carrierService.CreateAsync(request, getCurrentUser());
            return result.ToCreatedResult(this, nameof(GetCarrierById), new { id = result.Value?.Id });
        }

        [HttpPut("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateCarrier(Guid id, [FromBody] UpdateCarrierRequest request)
        {
            var result = await _carrierService.UpdateAsync(id, request, getCurrentUser());
            return result.ToActionResult(this);
        }

        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCarrier(Guid id)
        {
            var result = await _carrierService.DeleteAsync(id, getCurrentUser());
            if (result.IsSuccess) return Ok(new { message = "Carrier deleted successfully" });
            return result.ToActionResult(this);
        }

        private string getCurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not found");
    }
}
