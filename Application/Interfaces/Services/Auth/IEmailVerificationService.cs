using Application.DTOs.Auth;

namespace Application.Interfaces.Services.Auth
{
    public interface IEmailVerificationService
    {
        Task<AuthResponse> SendEmailConfirmationAsync(string userId);
        Task<AuthResponse> ConfirmEmailAsync(string userId, string token);
        Task<AuthResponse> ResendEmailConfirmationAsync(string email);
        Task SendChangeEmailConfirmationAsync(string userId, string newEmail);
    }
}
