using Application.DTOs.Shipments.Core;
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
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll([FromQuery] ShipmentParameters parameters)
        {
            var shipments = await _shipmentService.GetAllAsync(parameters);
            return Ok(shipments);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetAllForCurrentUser([FromQuery] ShipmentParameters parameters)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var shipments = await _shipmentService.GetAllForUserAsync(userId, parameters);
            return Ok(shipments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");
            var shipment = await _shipmentService.GetByIdAsync(id, userId, isPrivileged);
            if (shipment == null)
                return NotFound();

            return Ok(shipment);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var createdShipment = await _shipmentService.CreateAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = createdShipment.Id }, createdShipment);
        }

        [HttpPut("{id}")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShipmentRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var updatedShipment = await _shipmentService.UpdateAsync(id, userId, request);
            if (updatedShipment == null)
                return NotFound();

            return Ok(updatedShipment);
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var deleted = await _shipmentService.DeleteAsync(id, userId);
            if (!deleted)
                return NotFound();

            return Ok("shipment deleted successfully");
        }

        [HttpPatch("{id}/change-status")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeShipmentStatusRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");
            var updatedShipment = await _shipmentService.ChangeStatusAsync(id, userId, isPrivileged, request);
            if (updatedShipment == null)
                return NotFound();

            return Ok(updatedShipment);
        }
    }
}