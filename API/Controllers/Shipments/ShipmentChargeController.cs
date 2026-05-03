using Application.DTOs.Shipments.Core;
using Application.Interfaces.Services.Shipments.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Shipments
{
    [Route("api/[controller]")]
    [ApiController]
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
            var charge = await shipmentChargeService.GetByIdAsync(id);
            if (charge == null)
                return NotFound();

            return Ok(charge);
        }

        [HttpGet("shipment/{shipmentId}")]
        public async Task<IActionResult> GetByShipmentId(Guid shipmentId)
        {
            var charges = await shipmentChargeService.GetByShipmentIdAsync(shipmentId);
            if (!charges.Any())
                return NotFound();

            return Ok(charges);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateShipmentChargeRequest request)
        {
            var createdCharge = await shipmentChargeService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdCharge.Id }, createdCharge);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateShipmentChargeRequest request)
        {
            var updatedCharge = await shipmentChargeService.UpdateAsync(id, request);
            if (updatedCharge == null)
                return NotFound();

            return Ok(updatedCharge);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await shipmentChargeService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return Ok("Shipment charge deleted successfully.");
        }
    }
}
