using Application.DTOs.Pricing.Quotation;
using Application.Interfaces.Services.Pricing.Quotation;
using Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers.Pricing.Quotation
{
    [ApiController]
    [Route("api/quotes")]
    [EnableRateLimiting("ReadPolicy")]
    public class QuoteController : ControllerBase
    {
        private readonly IQuoteService _quoteService;

        public QuoteController(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllQuotes([FromQuery] QueryParameters query)
        {
            var quotes = await _quoteService.GetAllAsync(query);

            if (quotes == null || !quotes.Any())
                return NotFound(new { message = "No quotes found" });

            return Ok(quotes);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetQuoteById(Guid id)
        {
            var quote = await _quoteService.GetByIdAsync(id);

            if (quote == null)
                return NotFound(new { message = "Quote not found" });

            return Ok(quote);
        }

        [HttpGet("customer/{customerName}")]
        public async Task<IActionResult> GetQuotesByCustomerName(string customerName, [FromQuery] QueryParameters query)
        {
            var quotes = await _quoteService.GetByCustomerNameAsync(customerName, query);

            if (quotes == null || !quotes.Any())
                return NotFound(new { message = $"No quotes found for customer '{customerName}'" });

            return Ok(quotes);
        }

        [HttpGet("route/{routeId:guid}")]
        public async Task<IActionResult> GetQuotesByRouteId(Guid routeId, [FromQuery] QueryParameters query)
        {
            var quotes = await _quoteService.GetByRouteIdAsync(routeId, query);
            if (quotes == null || !quotes.Any())
                return NotFound(new { message = $"No quotes found for route ID '{routeId}'" });
            return Ok(quotes);
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteRequest request)
        {
            var result = await _quoteService.CreateAsync(request);

            return CreatedAtAction(nameof(GetQuoteById), new { id = result.Id }, result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteQuote(Guid id)
        {
            var exists = await _quoteService.GetByIdAsync(id);
            if (exists == null)
                return NotFound(new { message = "Quote not found" });

            await _quoteService.DeleteAsync(id);

            return Ok(new { message = "Quote deleted successfully" });
        }
    }
}