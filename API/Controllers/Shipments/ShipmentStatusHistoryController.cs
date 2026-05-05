using Application.Interfaces.Services.Shipments.Core;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.Shipments
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("ReadPolicy")]
    [Authorize]
    public class ShipmentStatusHistoryController : ControllerBase
    {
        private readonly IShipmentStatusHistoryService _shipmentStatusHistoryService;

        public ShipmentStatusHistoryController(IShipmentStatusHistoryService shipmentStatusHistoryService)
        {
            _shipmentStatusHistoryService = shipmentStatusHistoryService;
        }

        [HttpGet("{shipmentId}")]
        public async Task<IActionResult> GetByShipmentId(Guid shipmentId, [FromQuery] QueryParameters parameters)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");
            var history = await _shipmentStatusHistoryService
                .GetByShipmentIdAsync(shipmentId, userId, isPrivileged, parameters);
            if (!history.Any())
                return NotFound();

            return Ok(history);
        }
    }
}