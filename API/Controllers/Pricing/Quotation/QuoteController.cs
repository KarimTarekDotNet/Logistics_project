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
    [ApiController]
    [Route("api/quotes")]
    [EnableRateLimiting("ReadPolicy")]
    [Authorize]
    public class QuoteController : ControllerBase
    {
        private readonly IQuoteService _quoteService;

        public QuoteController(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAllQuotes([FromQuery] QueryParameters query)
        {
            var result = await _quoteService.GetAllAsync(query);
            return result.ToActionResult(this);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyQuotes([FromQuery] QueryParameters query)
        {
            var result = await _quoteService.GetMyQuotesAsync(getCurrentUser(), query);
            return result.ToActionResult(this);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetQuoteById(Guid id)
        {
            var isAdminOrStaff = User.IsInRole("Admin") || User.IsInRole("Staff");
            var result = await _quoteService.GetByIdAsync(id, getCurrentUser(), isAdminOrStaff);
            return result.ToActionResult(this);
        }

        [HttpGet("customer/{customerName}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetQuotesByCustomerName(string customerName, [FromQuery] QueryParameters query)
        {
            var result = await _quoteService.GetByCustomerNameAsync(customerName, query);
            return result.ToActionResult(this);
        }

        [HttpGet("route/{routeId:guid}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetQuotesByRouteId(Guid routeId, [FromQuery] QueryParameters query)
        {
            var result = await _quoteService.GetByRouteIdAsync(routeId, query);
            return result.ToActionResult(this);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteRequest request)
        {
            var result = await _quoteService.CreateAsync(request, getCurrentUser());
            return result.ToCreatedResult(this, nameof(GetQuoteById), new { id = result.Value?.Id });
        }

        [HttpPatch("{id}/accept-from-user")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AcceptFromUser(Guid id)
        {
            var result = await _quoteService.AcceptFromUserAsync(id, getCurrentUser());
            return result.ToActionResult(this);
        }

        [HttpPatch("{id}/rejected-from-user")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RejectedFromUser(Guid id, string reason)
        {
            var result = await _quoteService.RejectFromUserAsync(id, getCurrentUser(), reason);
            return result.ToActionResult(this);
        }

        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteQuote(Guid id)
        {
            var isAdmin = User.IsInRole("Admin");
            var result = await _quoteService.DeleteAsync(id, isAdmin, getCurrentUser());
            if (result.IsSuccess) return Ok(new { message = "Quote deleted successfully" });
            return result.ToActionResult(this);
        }

        private string getCurrentUser() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not found");
    }
}
