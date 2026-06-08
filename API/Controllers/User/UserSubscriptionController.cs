using Application.Interfaces.Repositories.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("ReadPolicy")]
    public class UserSubscriptionController : ControllerBase
    {
        private readonly IUserSubscriptionService _userSubscriptionService;

        public UserSubscriptionController(IUserSubscriptionService userSubscriptionService)
        {
            _userSubscriptionService = userSubscriptionService;
        }

        [HttpGet]
        [Authorize(Roles ="User")]
        public async Task<IActionResult> GetUserSubscriptionsAsync()
        {
            var userId = GetUserId();
            var Sub = await _userSubscriptionService.GetUserSubscriptionsAsync(userId);
            return Ok(new { Sub });
        }

        [HttpGet("current")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetCurrentUserSubscriptionsAsync()
        {
            var userId = GetUserId();
            var Sub = await _userSubscriptionService.GetCurrentUserSubscriptionsAsync(userId);
            return Ok(new { Sub });
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