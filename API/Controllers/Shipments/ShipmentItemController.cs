using Application.DTOs.Shipments.Core;
using Application.Interfaces.Services.Shipments.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Shipments
{
    [Route("api/[controller]")]
    [ApiController]
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
            var shipmentItem = await _shipmentItemService.GetByIdAsync(id);
            if (shipmentItem == null)
                return NotFound();

            return Ok(shipmentItem);
        }

        [HttpGet("shipment/{shipmentId}")]
        public async Task<IActionResult> GetByShipmentId(Guid shipmentId)
        {
            var shipmentItems = await _shipmentItemService.GetByShipmentIdAsync(shipmentId);
            if (!shipmentItems.Any())
                return NotFound();

            return Ok(shipmentItems);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateShipmentItemRequest request)
        {
            var createdItem = await _shipmentItemService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdItem.Id }, createdItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateShipmentItemRequest request)
        {
            var updatedItem = await _shipmentItemService.UpdateAsync(id, request);
            if (updatedItem == null)
                return NotFound();

            return Ok(updatedItem);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _shipmentItemService.DeleteAsync(id);
            if (!success)
                return NotFound();

            return Ok("Shipment item deleted successfully.");
        }
    }
}
