using Application.DTOs.Auth;

namespace Application.Interfaces.Services.Auth
{
    public interface IPhoneOtpService
    {
        Task SendOtpAsync(string phoneNumber);
        Task<bool> VerifyOtpAsync(string phoneNumber, string code);
        Task<AuthResponse> ResendAsync(string phone);
    }
}
