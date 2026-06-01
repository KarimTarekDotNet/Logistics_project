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
    [Authorize]
    [EnableRateLimiting("ReadPolicy")]
    public class ShipmentChargeController : ControllerBase
    {
        private readonly IShipmentChargeService shipmentChargeService;

        public ShipmentChargeController(IShipmentChargeService shipmentChargeService)
        {
            this.shipmentChargeService = shipmentChargeService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");

            var charge = await shipmentChargeService.GetByIdAsync(id, userId, isPrivileged);
            if (charge == null)
                return NotFound();

            return Ok(charge);
        }

        [HttpGet("shipment/{shipmentId}")]
        public async Task<IActionResult> GetByShipmentId(Guid shipmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");
            var charges = await shipmentChargeService.GetByShipmentIdAsync(shipmentId, userId, isPrivileged);
            if (!charges.Any())
                return NotFound();

            return Ok(charges);
        }

        [HttpPost("generate")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Generate(GenerateShipmentChargesRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var createdCharges = await shipmentChargeService.GenerateAsync(request, userId);
            return Ok(createdCharges);
        }

        [HttpPut("{id}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, UpdateShipmentChargeRequest request)
        {
            var updatedCharge = await shipmentChargeService.UpdateAsync(id, request);
            if (updatedCharge == null)
                return NotFound();

            return Ok(updatedCharge);
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");
            var deleted = await shipmentChargeService.DeleteAsync(id, userId, isPrivileged);
            if (!deleted)
                return NotFound();

            return Ok("Shipment charge deleted successfully.");
        }
    }
}
