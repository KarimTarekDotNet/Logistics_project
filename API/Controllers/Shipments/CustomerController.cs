using API.Extensions;
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
            var result = await _customerService.AddCustomerAsync(getCurrentUser(), request);
            return result.ToCreatedResult(this, nameof(GetMyCustomerProfile), new { });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyCustomerProfile()
        {
            var result = await _customerService.GetByApplicationUserIdAsync(getCurrentUser());
            return result.ToActionResult(this);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerRequest request)
        {
            var result = await _customerService.UpdateCustomerAsync(getCurrentUser(), request);
            return result.ToActionResult(this);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCustomer()
        {
            var result = await _customerService.DeleteCustomerAsync(getCurrentUser());
            if (result.IsSuccess) return Ok(new { message = "Customer deleted successfully." });
            return result.ToActionResult(this);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll([FromQuery] CustomerParameters parameters)
        {
            var result = await _customerService.GetAllAsync(parameters);
            return result.ToActionResult(this);
        }

        private string getCurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not found in token.");
    }
}
