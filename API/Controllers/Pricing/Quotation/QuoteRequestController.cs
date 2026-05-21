using Application.DTOs.Pricing.Quotation;
using Application.Interfaces.Services.Pricing.Quotation;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers.Pricing.Quotation
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("ReadPolicy")]
    public class QuoteRequestController : ControllerBase
    {
        private readonly IQuoteRequestService _quoteRequestService;

        public QuoteRequestController(IQuoteRequestService quoteRequestService)
        {
            _quoteRequestService = quoteRequestService;
        }

        [HttpPost("from-rate")]
        [Authorize(Roles = "User")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> CreateFromRate([FromBody] CreateQuoteRequestFromRate request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _quoteRequestService.CreateFromRateAsync(request, userId);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("my")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyRequests([FromQuery] QueryParameters query)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _quoteRequestService.GetMyRequestsAsync(userId, query);

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll([FromQuery] QueryParameters query)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _quoteRequestService.GetAllAsync(userId, query);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _quoteRequestService.GetByIdAsync(id));
        }

        [HttpPatch("{id:guid}/approve")]
        [Authorize(Roles = "Admin,Staff")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _quoteRequestService.ApproveAsync(id, userId);

            return Ok(result);
        }

        [HttpPatch("{id:guid}/reject")]
        [Authorize(Roles = "Admin,Staff")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectQuoteRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _quoteRequestService.RejectAsync(id, userId, request.Reason);

            return Ok(result);
        }

        [HttpPatch("{id:guid}/cancel")]
        [Authorize(Roles = "User")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> CancelByUser(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _quoteRequestService.CancelByUserAsync(id, userId);

            return Ok(result);
        }
    }
}
