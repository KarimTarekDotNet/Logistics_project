using API.Filters;
using API.Filters.Attributes;
using Application.DTOs.Pricing.Imports;
using Application.Interfaces.Services.Pricing.Imports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers.Pricing.Imports
{
    [Route("api/integrations/rates")]
    [ApiController]
    [EnableRateLimiting("HeavyPolicy")]
    public class RateImportController : ControllerBase
    {
        private readonly IRateImportService _rateImportService;

        public RateImportController(IRateImportService rateImportService)
        {
            _rateImportService = rateImportService;
        }

        [HttpPost("import")]
        [IntegrationKey]
        [Authorize(Roles = "Integration")]
        public async Task<IActionResult> Import([FromBody] ImportRatesRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _rateImportService.ImportAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}