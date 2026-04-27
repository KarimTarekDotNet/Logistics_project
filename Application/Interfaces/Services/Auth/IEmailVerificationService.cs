using Application.DTOs.Auth;

namespace Application.Interfaces.Services.Auth
{
    public interface IEmailVerificationService
    {
        Task<AuthResponse> SendEmailConfirmationAsync(string userId);
        Task<AuthResponse> ConfirmEmailAsync(string userId, string token);
        Task<AuthResponse> ResendEmailConfirmationAsync(string email);
    }
    public interface IPhoneOtpService
    {
        Task SendOtpAsync(string phoneNumber);
        Task<bool> VerifyOtpAsync(string phoneNumber, string code);
        Task<AuthResponse> ResendAsync(string phone);
    }
}
