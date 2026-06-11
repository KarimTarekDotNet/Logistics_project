using Application.DTOs.User;
using Application.Interfaces.Repositories.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("HeavyPolicy")]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly ISubscriptionPlanService _subscriptionPlanService;

        public SubscriptionPlanController(ISubscriptionPlanService subscriptionPlanService)
        {
            _subscriptionPlanService = subscriptionPlanService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAsync()
        {
            var plans = await _subscriptionPlanService.GetAllAsync();
            return Ok(plans);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var plan = await _subscriptionPlanService.GetByIdAsync(id);
            return Ok(plan);
        }

        [HttpPost]
        [Authorize(Roles ="Admin,Staff")]
        public async Task<IActionResult> AddFromEmployee([FromBody] CreateSubscriptionPlanRequest request)
        {
            var isInRole = User.IsInRole("Admin") || User.IsInRole("Staff");
            var newPlan = await _subscriptionPlanService.AddFromEmployeesAsync(request, isInRole);
            return Ok(newPlan);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteFromEmployee(Guid id)
        {
            var isInRole = User.IsInRole("Admin") || User.IsInRole("Staff");
            await _subscriptionPlanService.DeleteFromEmployeesAsync(id, isInRole);

            return Ok("Plan deleted successfully");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateFromEmployee(Guid id, [FromBody] UpdateSubscriptionPlanRequest request)
        {
            var isInRole = User.IsInRole("Admin") || User.IsInRole("Staff");
            var newPlan = await _subscriptionPlanService.UpdateFromEmployeesAsync(id, request, isInRole);

            return Ok(newPlan);
        }
    }
}