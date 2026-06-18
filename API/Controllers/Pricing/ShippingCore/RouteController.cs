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
            var routes = await _routeService.GetAllAsync(query);

            if (routes == null || !routes.Any())
                return NotFound(new { message = "No routes found" });

            return Ok(routes);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRouteById(Guid id)
        {
            var route = await _routeService.GetByIdAsync(id);

            if (route == null)
                return NotFound(new { message = "Route not found" });

            return Ok(route);
        }

        [HttpGet("from-port/{fromPortId:guid}")]
        public async Task<IActionResult> GetRoutesByFromPort(Guid fromPortId, [FromQuery] QueryParameters query)
        {
            var routes = await _routeService.GetByFromPortAsync(fromPortId, query);

            if (routes == null || !routes.Any())
                return NotFound(new { message = "No routes found for the specified port" });

            return Ok(routes);
        }

        [HttpGet("to-port/{toPortId:guid}")]
        public async Task<IActionResult> GetRoutesByToPort(Guid toPortId, [FromQuery] QueryParameters query)
        {
            var routes = await _routeService.GetByToPortAsync(toPortId, query);

            if (routes == null || !routes.Any())
                return NotFound(new { message = "No routes found for the specified port" });

            return Ok(routes);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateRoute([FromBody] CreateRouteRequest request)
        {
            var result = await _routeService.CreateAsync(request, getCurrentUser());

            return CreatedAtAction(nameof(GetRouteById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateRoute(Guid id, [FromBody] UpdateRouteRequest request)
        {
            var updated = await _routeService.UpdateAsync(id, request, getCurrentUser());

            if (updated == null)
                return NotFound(new { message = "Route not found" });

            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoute(Guid id)
        {
            await _routeService.DeleteAsync(id, getCurrentUser());

            return Ok(new { message = "Route deleted successfully" });
        }

        private string getCurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("user not found");
    }
}