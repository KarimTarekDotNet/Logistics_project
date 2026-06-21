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
    [Route("api/routes")]
    [EnableRateLimiting("ReadPolicy")]
    public class RouteController : ControllerBase
    {
        private readonly IRouteService _routeService;

        public RouteController(IRouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoutes([FromQuery] QueryParameters query)
        {
            var result = await _routeService.GetAllAsync(query);
            return result.ToActionResult(this);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRouteById(Guid id)
        {
            var result = await _routeService.GetByIdAsync(id);
            return result.ToActionResult(this);
        }

        [HttpGet("from-port/{fromPortId:guid}")]
        public async Task<IActionResult> GetRoutesByFromPort(Guid fromPortId, [FromQuery] QueryParameters query)
        {
            var result = await _routeService.GetByFromPortAsync(fromPortId, query);
            return result.ToActionResult(this);
        }

        [HttpGet("to-port/{toPortId:guid}")]
        public async Task<IActionResult> GetRoutesByToPort(Guid toPortId, [FromQuery] QueryParameters query)
        {
            var result = await _routeService.GetByToPortAsync(toPortId, query);
            return result.ToActionResult(this);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateRoute([FromBody] CreateRouteRequest request)
        {
            var result = await _routeService.CreateAsync(request, getCurrentUser());
            return result.ToCreatedResult(this, nameof(GetRouteById), new { id = result.Value?.Id });
        }

        [HttpPut("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateRoute(Guid id, [FromBody] UpdateRouteRequest request)
        {
            var result = await _routeService.UpdateAsync(id, request, getCurrentUser());
            return result.ToActionResult(this);
        }

        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoute(Guid id)
        {
            var result = await _routeService.DeleteAsync(id, getCurrentUser());
            if (result.IsSuccess) return Ok(new { message = "Route deleted successfully" });
            return result.ToActionResult(this);
        }

        private string getCurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not found");
    }
}
