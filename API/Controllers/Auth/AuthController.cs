using Application.DTOs.Auth;
using Application.Interfaces.Services.Auth;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

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
        private readonly IAntiforgery _antiforgery;

        public AuthController(IAuthService auth, IEmailVerificationService emailVerificationService,
        IPhoneOtpService phoneOtpService, IAntiforgery antiforgery)
        {
            this.auth = auth;
            _emailVerificationService = emailVerificationService;
            _phoneOtpService = phoneOtpService;
            _antiforgery = antiforgery;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var loginResult = await auth.LoginAsync(request, ipAddress);

            if (!loginResult.IsAuthenticated)
                return Unauthorized(loginResult);

            SetJwtCookie(loginResult.AccessToken!);
            SetRefreshTokenCookie(loginResult.RefreshToken!);

            return Ok(new
            {
                loginResult.IsAuthenticated,
                loginResult.UserName,
                loginResult.Email
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            var registerResult = await auth.RegisterAsync(request);

            if (registerResult.Id is null)
                return BadRequest(registerResult);

            return Ok(registerResult);
        }

        [HttpPost("confirm-phone")]
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

            SetJwtCookie(result.AccessToken!);
            SetRefreshTokenCookie(result.RefreshToken!);

            return Ok(new
            {
                result.IsAuthenticated,
                result.UserName,
                result.Email
            });
        }

        [HttpPost("resend-email-confirmation")]
        public async Task<IActionResult> ResendEmailConfirmation([FromQuery] ResendEmailConfirmationRequest request)
        {
            var result = await _emailVerificationService.ResendEmailConfirmationAsync(request.Email);

            if (!result.IsAuthenticated)
                return BadRequest(result);

            SetJwtCookie(result.AccessToken!);
            SetRefreshTokenCookie(result.RefreshToken!);

            return Ok(new
            {
                result.IsAuthenticated,
                result.UserName,
                result.Email
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["RefreshToken"];

            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized();

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await auth.RefreshAsync(refreshToken, ipAddress);
            if (!result.IsAuthenticated)
                return Unauthorized(result);

            SetJwtCookie(result.AccessToken!);
            SetRefreshTokenCookie(result.RefreshToken!);

            return Ok(new
            {
                result.IsAuthenticated,
                result.UserName,
                result.Email
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["RefreshToken"];

            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized();

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await auth.LogoutAsync(refreshToken, ipAddress);
            if (!result)
                return BadRequest(new { Message = "Failed to logout" });

            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("RefreshToken");

            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var currentUserId = GetUserId();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await auth.LogoutAllAsync(currentUserId, ipAddress);
            if (!result)
                return BadRequest(new { Message = "Failed to logout from all sessions" });

            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("RefreshToken");
                
            return Ok(new { Message = "Logged out from all sessions successfully" });
        }

        [HttpGet("csrf-token")]
        public IActionResult GetCsrfToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

            Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
                new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    IsEssential = true
                });

            return Ok();
        }

        private string GetUserId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User id claim not found.");

            return userId;
        }

        private void SetJwtCookie(string jwtToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            };

            HttpContext.Response.Cookies.Append("AuthToken", jwtToken, cookieOptions);
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(15)
            };

            HttpContext.Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
        }
    }
}
