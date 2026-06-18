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
            var ports = await _portService.GetAllAsync(query);

            if (ports == null || !ports.Any())
                return NotFound(new { message = "No ports found" });

            return Ok(ports);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPortById(Guid id)
        {
            var port = await _portService.GetByIdAsync(id);

            if (port == null)
                return NotFound(new { message = "Port not found" });

            return Ok(port);
        }

        [HttpGet("country")]
        public async Task<IActionResult> GetPortsByCountry([FromQuery] string country, [FromQuery] QueryParameters query)
        {
            var ports = await _portService.GetByCountryAsync(country, query);

            if (ports == null || !ports.Any())
                return NotFound(new { message = $"No ports found for country '{country}'" });

            return Ok(ports);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreatePort([FromBody] CreatePortRequest request)
        {
            var result = await _portService.CreateAsync(request, getCurrentUser());

            return CreatedAtAction(nameof(GetPortById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdatePort(Guid id, [FromBody] UpdatePortRequest request)
        {
            var updated = await _portService.UpdateAsync(id, request, getCurrentUser());

            if (updated == null)
                return NotFound(new { message = "Port not found" });

            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePort(Guid id)
        {
            await _portService.DeleteAsync(id, getCurrentUser());

            return Ok(new { message = "Port deleted successfully" });
        }

        private string getCurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("user not found");
    }
}