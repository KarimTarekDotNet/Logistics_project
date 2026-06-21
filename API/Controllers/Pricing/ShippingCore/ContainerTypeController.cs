using API.Extensions;
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
            var result = await _containerTypeService.GetAllAsync(query);
            return result.ToActionResult(this);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetContainerTypeById(Guid id)
        {
            var result = await _containerTypeService.GetByIdAsync(id);
            return result.ToActionResult(this);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateContainerType([FromBody] CreateContainerTypeRequest request)
        {
            var result = await _containerTypeService.CreateAsync(request, getCurrentUser());
            return result.ToCreatedResult(this, nameof(GetContainerTypeById), new { id = result.Value?.Id });
        }

        [HttpPut("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateContainerType(Guid id, [FromBody] UpdateContainerTypeRequest request)
        {
            var result = await _containerTypeService.UpdateAsync(id, request, getCurrentUser());
            return result.ToActionResult(this);
        }

        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteContainerType(Guid id)
        {
            var result = await _containerTypeService.DeleteAsync(id, getCurrentUser());
            if (result.IsSuccess) return Ok(new { message = "Container type deleted successfully" });
            return result.ToActionResult(this);
        }

        private string getCurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not found");
    }
}
