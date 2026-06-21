using API.Extensions;
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
            var result = await _quoteRequestService.CreateFromRateAsync(request, getCurrentUser());
            return result.ToCreatedResult(this, nameof(GetById), new { id = result.Value?.Id });
        }

        [HttpGet("my")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyRequests([FromQuery] QueryParameters query)
        {
            var result = await _quoteRequestService.GetMyRequestsAsync(getCurrentUser(), query);
            return result.ToActionResult(this);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll([FromQuery] QueryParameters query)
        {
            var result = await _quoteRequestService.GetAllAsync(getCurrentUser(), query);
            return result.ToActionResult(this);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _quoteRequestService.GetByIdAsync(id);
            return result.ToActionResult(this);
        }

        [HttpPatch("{id:guid}/approve")]
        [Authorize(Roles = "Admin,Staff")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var result = await _quoteRequestService.ApproveAsync(id, getCurrentUser());
            return result.ToActionResult(this);
        }

        [HttpPatch("{id:guid}/reject")]
        [Authorize(Roles = "Admin,Staff")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectQuoteRequest request)
        {
            var result = await _quoteRequestService.RejectAsync(id, getCurrentUser(), request.Reason);
            return result.ToActionResult(this);
        }

        [HttpPatch("{id:guid}/cancel")]
        [Authorize(Roles = "User")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> CancelByUser(Guid id)
        {
            var result = await _quoteRequestService.CancelByUserAsync(id, getCurrentUser());
            return result.ToActionResult(this);
        }

        private string getCurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not found");
    }
}
