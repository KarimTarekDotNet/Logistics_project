using Application.DTOs.Auth;
using Application.Interfaces.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("AuthPolicy")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService auth;
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly IPhoneOtpService _phoneOtpService;

        public AuthController(IAuthService auth, IEmailVerificationService emailVerificationService, IPhoneOtpService phoneOtpService)
        {
            this.auth = auth;
            _emailVerificationService = emailVerificationService;
            _phoneOtpService = phoneOtpService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var loginResult = await auth.LoginAsync(request, ipAddress);

            if (!loginResult.IsAuthenticated)
                return Unauthorized(loginResult);

            return Ok(loginResult);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            var registerResult = await auth.RegisterAsync(request);

            if (registerResult.Id is null)
                return BadRequest(registerResult);

            return Ok(registerResult);
        }

        [HttpGet("confirm-phone")]
        public async Task<IActionResult> ConfirmPhone([FromBody] ConfirmPhoneRequest request)
        {
            var result = await auth.ConfirmPhoneAsync(request);

            if (!result.IsAuthenticated)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("resend-phone-otp")]
        [EnableRateLimiting("OtpPolicy")]
        public async Task<IActionResult> ResendPhoneOtp([FromQuery] string phone)
        {
            var result = await _phoneOtpService.ResendAsync(phone);
            return Ok(result);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await _emailVerificationService.ConfirmEmailAsync(userId, token);

            if (!result.IsAuthenticated)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("resend-email-confirmation")]
        public async Task<IActionResult> ResendEmailConfirmation([FromQuery] ResendEmailConfirmationRequest request)
        {
            var result = await _emailVerificationService.ResendEmailConfirmationAsync(request.Email);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromQuery] RefreshTokenRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await auth.RefreshAsync(request, ipAddress);
            if (!result.IsAuthenticated)
                return Unauthorized(result);
            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] RefreshTokenRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await auth.LogoutAsync(request, ipAddress);
            if (!result)
                return BadRequest(new { Message = "Failed to logout" });
            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll([FromQuery] string userId)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await auth.LogoutAllAsync(userId, ipAddress);
            if (!result)
                return BadRequest(new { Message = "Failed to logout from all sessions" });
            return Ok(new { Message = "Logged out from all sessions successfully" });
        }
    }
}