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
            var quotes = await _quoteService.GetAllAsync(query);

            if (quotes == null || !quotes.Any())
                return NotFound(new { message = "No quotes found" });

            return Ok(quotes);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyQuotes([FromQuery] QueryParameters query)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var quotes = await _quoteService.GetMyQuotesAsync(userId, query);

            if (quotes == null || !quotes.Any())
                return NotFound(new { message = "No quotes found" });

            return Ok(quotes);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetQuoteById(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdminOrStaff = User.IsInRole("Admin") || User.IsInRole("Staff");

            var quote = await _quoteService.GetByIdAsync(id, userId, isAdminOrStaff);

            if (quote == null)
                return NotFound(new { message = "Quote not found" });

            return Ok(quote);
        }

        [HttpGet("customer/{customerName}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetQuotesByCustomerName(string customerName, [FromQuery] QueryParameters query)
        {
            var quotes = await _quoteService.GetByCustomerNameAsync(customerName, query);

            if (quotes == null || !quotes.Any())
                return NotFound(new { message = $"No quotes found for customer '{customerName}'" });

            return Ok(quotes);
        }

        [HttpGet("route/{routeId:guid}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetQuotesByRouteId(Guid routeId, [FromQuery] QueryParameters query)
        {
            var quotes = await _quoteService.GetByRouteIdAsync(routeId, query);
            if (quotes == null || !quotes.Any())
                return NotFound(new { message = $"No quotes found for route ID '{routeId}'" });
            return Ok(quotes);
        }

        [HttpPost]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteRequest request)
        {
            var result = await _quoteService.CreateAsync(request);

            return CreatedAtAction(nameof(GetQuoteById), new { id = result.Id }, result);
        }

        [HttpPatch("{id}/accept-from-user")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AcceptFromUser(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _quoteService.AcceptFromUserAsync(id, userId);

            return Ok(result);
        }

        [HttpPatch("{id}/rejected-from-user")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RejectedFromUser(Guid id, string reason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _quoteService.RejectFromUserAsync(id, userId, reason);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [EnableRateLimiting("HeavyPolicy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteQuote(Guid id)
        {
            var isAdmin = User.IsInRole("Admin");

            await _quoteService.DeleteAsync(id, isAdmin);

            return Ok(new { message = "Quote deleted successfully" });
        }
    }
}