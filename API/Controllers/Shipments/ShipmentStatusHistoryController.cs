using Application.Interfaces.Services.Shipments.Core;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Shipments
{
    [Route("api/[controller]")]
    [ApiController]
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
            var history = await _shipmentStatusHistoryService.GetByShipmentIdAsync(shipmentId, parameters);
            if (!history.Any())
                return NotFound();

            return Ok(history);
        }
    }
}
