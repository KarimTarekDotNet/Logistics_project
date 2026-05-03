using Application.DTOs.Shipments.API;

namespace Application.Interfaces.Services.Shipments.ApisIntegrations
{
    public interface ITaxVerificationService
    {
        Task<TaxVerificationResult> VerifyAsync(string countryCode, string taxNumber, CancellationToken cancellationToken = default);
    }
}
