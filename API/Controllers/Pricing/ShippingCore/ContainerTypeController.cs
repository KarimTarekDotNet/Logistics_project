using Application.DTOs.ShippingCore;
using Application.Interfaces.Services.Pricing.ShippingCore;
using Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers.Pricing.ShippingCore
{
    [ApiController]
    [Route("api/container-types")]
    [EnableRateLimiting("ReadPolicy")]
    public class ContainerTypeController : ControllerBase
    {
        private readonly IContainerTypeService _containerTypeService;

        public ContainerTypeController(IContainerTypeService containerTypeService)
        {
            _containerTypeService = containerTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllContainerTypes([FromQuery] QueryParameters query)
        {
            var containerTypes = await _containerTypeService.GetAllAsync(query);

            if (containerTypes == null || !containerTypes.Any())
                return NotFound(new { message = "No container types found" });

            return Ok(containerTypes);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetContainerTypeById(Guid id)
        {
            var containerType = await _containerTypeService.GetByIdAsync(id);

            if (containerType == null)
                return NotFound(new { message = "Container type not found" });

            return Ok(containerType);
        }

        [HttpPost]
        public async Task<IActionResult> CreateContainerType([FromBody] CreateContainerTypeRequest request)
        {
            var result = await _containerTypeService.CreateAsync(request);

            return CreatedAtAction(nameof(GetContainerTypeById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateContainerType(Guid id, [FromBody] UpdateContainerTypeRequest request)
        {
            var updated = await _containerTypeService.UpdateAsync(id, request);

            if (updated == null)
                return NotFound(new { message = "Container type not found" });

            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteContainerType(Guid id)
        {
            var exists = await _containerTypeService.GetByIdAsync(id);
            if (exists == null)
                return NotFound(new { message = "Container type not found" });

            await _containerTypeService.DeleteAsync(id);

            return Ok(new { message = "Container type deleted successfully" });
        }
    }
}