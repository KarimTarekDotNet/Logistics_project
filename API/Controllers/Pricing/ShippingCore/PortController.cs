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
    [Route("api/ports")]
    [EnableRateLimiting("ReadPolicy")]
    public class PortController : ControllerBase
    {
        private readonly IPortService _portService;

        public PortController(IPortService portService)
        {
            _portService = portService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPorts([FromQuery] QueryParameters query)
        {
            var result = await _portService.GetAllAsync(query);
            return result.ToActionResult(this);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPortById(Guid id)
        {
            var result = await _portService.GetByIdAsync(id);
            return result.ToActionResult(this);
        }

        [HttpGet("country")]
        public async Task<IActionResult> GetPortsByCountry([FromQuery] string country, [FromQuery] QueryParameters query)
        {
            var result = await _portService.GetByCountryAsync(country, query);
            return result.ToActionResult(this);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreatePort([FromBody] CreatePortRequest request)
        {
            var result = await _portService.CreateAsync(request, getCurrentUser());
            return result.ToCreatedResult(this, nameof(GetPortById), new { id = result.Value?.Id });
        }

        [HttpPut("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdatePort(Guid id, [FromBody] UpdatePortRequest request)
        {
            var result = await _portService.UpdateAsync(id, request, getCurrentUser());
            return result.ToActionResult(this);
        }

        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePort(Guid id)
        {
            var result = await _portService.DeleteAsync(id, getCurrentUser());
            if (result.IsSuccess) return Ok(new { message = "Port deleted successfully" });
            return result.ToActionResult(this);
        }

        private string getCurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not found");
    }
}
