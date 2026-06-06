using Application.DTOs.Auth;
using Application.Interfaces.Services.Auth;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace Infrastructure.Services.Auth
{
    public class TwilioPhoneOtpService : IPhoneOtpService
    {
        private readonly IConfiguration _config;
        public TwilioPhoneOtpService(IConfiguration config)
        {
            _config = config;
            TwilioClient.Init(_config.GetValue<string>("Twilio:AccountSid"), _config.GetValue<string>("Twilio:AuthToken"));
        }

        public async Task SendOtpAsync(string phoneNumber)
        {
            await VerificationResource.CreateAsync(to: phoneNumber, channel: "sms", pathServiceSid: _config.GetValue<string>("Twilio:VerifyServiceSid"));
        }

        public async Task<AuthResponse> ResendAsync(string phone)
        {
            await VerificationResource.CreateAsync(to: phone, channel: "sms", pathServiceSid: _config.GetValue<string>("Twilio:VerifyServiceSid"));
            return new AuthResponse
            {
                Message = "The code was successfully resent."
            };
        }

        public async Task<bool> VerifyOtpAsync(string phoneNumber, string code)
        {
            var result = await VerificationCheckResource.CreateAsync( to: phoneNumber, code: code,
                pathServiceSid: _config.GetValue<string>("Twilio:VerifyServiceSid"));

            return result.Status == "approved";
        }
    }
}
