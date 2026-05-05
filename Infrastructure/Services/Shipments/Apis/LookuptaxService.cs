using Application.DTOs.Shipments.API;
using Application.Interfaces.Services.Shipments.ApisIntegrations;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Infrastructure.Services.Shipments.Apis
{
    public class LookuptaxService : ITaxVerificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public LookuptaxService(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }

        public async Task<TaxVerificationResult> VerifyAsync(string countryCode, string taxNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"{_config["TaxVerification:Lookuptax:BaseUrl"]}?country_iso={countryCode}&tin={taxNumber}";
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                request.Headers.Add("X-API-Key", _config["TaxVerification:Lookuptax:ApiKey"]);
                var response = await _httpClient.SendAsync(request, cancellationToken);

                if(!response.IsSuccessStatusCode)
                    return TaxVerificationResult.Failed("Tax verification failed.");

                var data = await response.Content.ReadFromJsonAsync<LookuptaxResponse>(cancellationToken: cancellationToken);

                if (data is null)
                    return TaxVerificationResult.Failed("Empty response from tax verification provider.");

                return new TaxVerificationResult
                {
                    IsValid = data.Validation?.Overall?.IsValid ?? false,
                    IsVerified = true,
                    Provider = "Lookuptax",
                    Message = data.Validation?.Overall?.Message,
                    ReferenceId = data.ReferenceId
                };
            }
            catch (TaskCanceledException)
            {
                return TaxVerificationResult.Failed("Tax verification timed out.");
            }
            catch (HttpRequestException)
            {
                return TaxVerificationResult.Failed("Tax verification provider is unavailable.");
            }
        }
    }
}
