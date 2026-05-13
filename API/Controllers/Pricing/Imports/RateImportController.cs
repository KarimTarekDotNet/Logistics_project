using API.Filters.Attributes;
using Application.DTOs.Pricing.Imports;
using Application.Interfaces.Services.Pricing.Imports;
using Domain.Enums;
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
        public async Task<IActionResult> Import([FromBody] ImportRatesRequest request, CancellationToken cancellationToken = default)
        {
            var context =
            new IntegrationRequestContext(Enum.Parse<ExternalSource>(HttpContext.Items["IntegrationSource"]!.ToString()!, true));

            var result = await _rateImportService.ImportAsync(request, context, cancellationToken);
            return Ok(result);
        }
    }
}