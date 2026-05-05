using Application.DTOs.User;
using Application.Interfaces.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.User
{
    [Route("api/user/profile")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("HeavyPolicy")]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [EnableRateLimiting("ReadPolicy")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();

            var result = await _userService.GetProfileAsync(userId);

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetUserId();

            var result = await _userService.UpdateProfileAsync(userId, request);

            return Ok(result);
        }

        [HttpPut("password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            var userId = GetUserId();

            var result = await _userService.UpdatePasswordAsync(userId, request);

            return Ok(new
            {
                success = result,
                message = "Password updated successfully."
            });
        }

        [HttpGet("confirm-email-change")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmailChange(
            [FromQuery] string userId,
            [FromQuery] string token)
        {
            var result = await _userService.ConfirmPendingEmailAsync(userId, token);

            return Ok(result);
        }

        [HttpPost("verify-phone-change")]
        public async Task<IActionResult> VerifyPhoneChange([FromBody] VerifyPendingPhoneRequest request)
        {
            var userId = GetUserId();

            var result = await _userService.VerifyPendingPhoneAsync(userId, request.Code);

            return Ok(result);
        }

        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User id claim not found.");

            return userId;
        }
    }
}