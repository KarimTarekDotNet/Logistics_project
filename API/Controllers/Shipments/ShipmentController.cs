using Application.DTOs.Shipments.Core;
using Application.Interfaces.Services.Shipments.Core;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Shipments
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ShipmentParameters parameters)
        {
            var shipments = await _shipmentService.GetAllAsync(parameters);
            return Ok(shipments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var shipment = await _shipmentService.GetByIdAsync(id);
            if (shipment == null)
                return NotFound();

            return Ok(shipment);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] CreateShipmentRequest request)
        {
            var createdShipment = await _shipmentService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdShipment.Id }, createdShipment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromQuery] UpdateShipmentRequest request)
        {
            var updatedShipment = await _shipmentService.UpdateAsync(id, request);
            if (updatedShipment == null)
                return NotFound();

            return Ok(updatedShipment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _shipmentService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return Ok("shipment deleted successfully");
        }

        [HttpPatch("{id}/change-status")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromQuery] ChangeShipmentStatusRequest request)
        {
            var updatedShipment = await _shipmentService.ChangeStatusAsync(id, request);
            if (updatedShipment == null)
                return NotFound();

            return Ok(updatedShipment);
        }
    }
}