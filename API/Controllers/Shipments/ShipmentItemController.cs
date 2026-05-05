using Application.DTOs.Shipments.Core;
using Application.Interfaces.Services.Shipments.Core;
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");
            var shipmentItem = await _shipmentItemService.GetByIdAsync(id, userId, isPrivileged);
            if (shipmentItem == null)
                return NotFound();

            return Ok(shipmentItem);
        }

        [HttpGet("shipment/{shipmentId}")]
        public async Task<IActionResult> GetByShipmentId(Guid shipmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");
            var shipmentItems = await _shipmentItemService.GetByShipmentIdAsync(shipmentId, userId, isPrivileged);
            if (!shipmentItems.Any())
                return NotFound();

            return Ok(shipmentItems);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Create([FromBody] CreateShipmentItemRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var createdItem = await _shipmentItemService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = createdItem.Id }, createdItem);
        }

        [HttpPut("{id}")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShipmentItemRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var updatedItem = await _shipmentItemService.UpdateAsync(id, userId, request);
            if (updatedItem == null)
                return NotFound();

            return Ok(updatedItem);
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var success = await _shipmentItemService.DeleteAsync(id, userId);
            if (!success)
                return NotFound();

            return Ok("Shipment item deleted successfully.");
        }
    }
}
