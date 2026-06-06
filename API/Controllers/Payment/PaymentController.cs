using Application.DTOs.Payment;
using Application.Interfaces.Services.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.Payment
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("HeavyPolicy")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentTransactionService _paymentTransactionService;

        public PaymentController(IPaymentTransactionService paymentTransactionService)
        {
            _paymentTransactionService = paymentTransactionService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentTransactionById(Guid id)
        {
            var userIdFromToken = GetUserId();
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff");
            var result = await _paymentTransactionService.GetByIdAsync(id, userIdFromToken, isPrivileged);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartPayment([FromBody] StartPaymentRequest request)
        {
            var userIdFromToken = GetUserId();
            var result = await _paymentTransactionService.StartPaymentAsync(request, userIdFromToken);
            return Ok(result);
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook([FromBody] PaymobTransactionWebhookRequest request)
        {
            string receivedHmac = Request.Query["hmac"].FirstOrDefault()!;
            if(receivedHmac == null)
                return BadRequest("HMAC signature is missing.");

            await _paymentTransactionService.HandlePaymobWebhookAsync(request, receivedHmac);
            return Ok();
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelPayment([FromQuery] Guid paymentTransactionId)
        {
            var userIdFromToken = GetUserId();
            await _paymentTransactionService.CancelPendingPaymentAsync(paymentTransactionId, userIdFromToken);
            return Ok();
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CheckoutPayment([FromQuery] Guid paymentTransactionId)
        {
            var userIdFromToken = GetUserId();
            var result = await _paymentTransactionService.CheckoutAsync(paymentTransactionId, userIdFromToken);
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
