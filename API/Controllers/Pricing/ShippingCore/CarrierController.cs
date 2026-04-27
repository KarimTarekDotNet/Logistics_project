using Application.DTOs.ShippingCore;
using Application.Interfaces.Services.Pricing.ShippingCore;
using Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers.Pricing.ShippingCore
{
    [ApiController]
    [Route("api/carriers")]
    [EnableRateLimiting("ReadPolicy")]
    public class CarrierController : ControllerBase
    {
        private readonly ICarrierService _carrierService;

        public CarrierController(ICarrierService carrierService)
        {
            _carrierService = carrierService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCarriers([FromQuery] QueryParameters query)
        {
            var carriers = await _carrierService.GetAllAsync(query);

            if (carriers == null || !carriers.Any())
                return NotFound(new { message = "No carriers found" });

            return Ok(carriers);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCarrierById(Guid id)
        {
            var carrier = await _carrierService.GetByIdAsync(id);

            if (carrier == null)
                return NotFound(new { message = "Carrier not found" });

            return Ok(carrier);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCarrier([FromBody] CreateCarrierRequest request)
        {
            var result = await _carrierService.CreateAsync(request);

            return CreatedAtAction(nameof(GetCarrierById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCarrier(Guid id, [FromBody] UpdateCarrierRequest request)
        {
            var updated = await _carrierService.UpdateAsync(id, request);

            if (updated == null)
                return NotFound(new { message = "Carrier not found" });

            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCarrier(Guid id)
        {
            var exists = await _carrierService.GetByIdAsync(id);
            if (exists == null)
                return NotFound(new { message = "Carrier not found" });

            await _carrierService.DeleteAsync(id);

            return Ok(new { message = "Carrier deleted successfully" });
        }
    }
}