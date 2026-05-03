using Application.DTOs.Shipments.User;
using Application.Interfaces.Services.Shipments.User;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers.Shipments
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomer([FromBody] CreateCustomerRequest request)
        {
            var userId = GetUserId();

            var result = await _customerService.AddCustomerAsync(userId, request);

            return CreatedAtAction(nameof(GetMyCustomerProfile), new { }, result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyCustomerProfile()
        {
            var userId = GetUserId();

            var result = await _customerService.GetByApplicationUserIdAsync(userId);

            if (result == null)
                return NotFound("Customer profile not found.");

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerRequest request)
        {
            var userId = GetUserId();

            var result = await _customerService.UpdateCustomerAsync(userId, request);

            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCustomer()
        {
            var userId = GetUserId();

            await _customerService.DeleteCustomerAsync(userId);

            return Ok("Customer deleted successfully.");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll([FromQuery] CustomerParameters parameters)
        {
            var result = await _customerService.GetAllAsync(parameters);

            return Ok(result);
        }

        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User id was not found in token.");

            return userId;
        }
    }
}