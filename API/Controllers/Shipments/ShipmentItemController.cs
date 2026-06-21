using API.Extensions;
using Application.DTOs.Shipments.Core;
using Application.Interfaces.Services.Shipments.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.Shipments
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("ReadPolicy")]
    [Authorize]
    public class ShipmentItemController : ControllerBase
    {
        private readonly IShipmentItemService _shipmentItemService;

        public ShipmentItemController(IShipmentItemService shipmentItemService)
        {
            _shipmentItemService = shipmentItemService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();
            var result = await _shipmentItemService.GetByIdAsync(id, userId, isPrivileged);
            return result.ToActionResult(this);
        }

        [HttpGet("shipment/{shipmentId}")]
        public async Task<IActionResult> GetByShipmentId(Guid shipmentId)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();
            var result = await _shipmentItemService.GetByShipmentIdAsync(shipmentId, userId, isPrivileged);
            return result.ToActionResult(this);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Create([FromBody] CreateShipmentItemRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();
            var result = await _shipmentItemService.CreateAsync(request, userId, isPrivileged);
            return result.ToCreatedResult(this, nameof(GetById), new { id = result.Value?.Id });
        }

        [HttpPut("{id}")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShipmentItemRequest request)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();
            var result = await _shipmentItemService.UpdateAsync(id, userId, isPrivileged, request);
            return result.ToActionResult(this);
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var (userId, isPrivileged) = GetCurrentUserContext();
            var result = await _shipmentItemService.DeleteAsync(id, userId, isPrivileged);
            if (result.IsSuccess) return Ok(new { message = "Shipment item deleted successfully." });
            return result.ToActionResult(this);
        }

        private (string userId, bool isPrivileged) GetCurrentUserContext()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");
            return (userId, isPrivileged);
        }
    }
}
