using Application.DTOs.ShippingCore;
using Application.Interfaces.Services.Pricing.ShippingCore;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

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
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateContainerType([FromBody] CreateContainerTypeRequest request)
        {
            var result = await _containerTypeService.CreateAsync(request, getCurrentUser());

            return CreatedAtAction(nameof(GetContainerTypeById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateContainerType(Guid id, [FromBody] UpdateContainerTypeRequest request)
        {
            var updated = await _containerTypeService.UpdateAsync(id, request, getCurrentUser());

            if (updated == null)
                return NotFound(new { message = "Container type not found" });

            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteContainerType(Guid id)
        {
            await _containerTypeService.DeleteAsync(id, getCurrentUser());

            return Ok(new { message = "Container type deleted successfully" });
        }

        private string getCurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("user not found");
    }
}