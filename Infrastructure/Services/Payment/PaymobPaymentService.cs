using Application.DTOs.Payment;
using Application.Interfaces.Services.Payment;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Infrastructure.Services.Payment
{
    public class PaymobPaymentService : IPaymobPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public PaymobPaymentService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<CreatePaymobIntentionResponse> CreateIntentionAsync(CreatePaymobIntentionRequest request)
        {
            var baseUrl = _configuration.GetValue<string>("Paymob:BaseUrl");
            var url = $"{baseUrl}/v1/intention/";
            _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Token", _configuration.GetValue<string>("Paymob:SecretKey"));
            var response = await _httpClient.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Paymob error: {responseBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<CreatePaymobIntentionResponse>();

            if(result == null)
                throw new InvalidOperationException("Failed to parse Paymob response.");

            return result;
        }
    }
}
