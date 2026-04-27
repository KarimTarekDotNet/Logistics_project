using Application.DTOs.Pricing.Imports;

namespace Application.Interfaces.Services.Pricing.Imports
{
    public interface IRateImportService
    {
        Task<ImportRatesResponse> ImportAsync(ImportRatesRequest request, CancellationToken cancellationToken = default);
    }
}
