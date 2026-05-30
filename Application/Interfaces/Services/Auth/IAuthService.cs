using Application.DTOs.Auth;
using Domain.Entities.Users;
using System.Net;

namespace Application.Interfaces.Services.Auth
{
    public interface IAuthService
    {
        Task<string> GenerateJwtToken(ApplicationUser user);
        Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> ConfirmPhoneAsync(ConfirmPhoneRequest request);
        Task<AuthResponse> RefreshAsync(string refreshToken, string? ipAddress);
        Task<bool> LogoutAsync(string refreshToken, string? ipAddress);
        Task<bool> LogoutAllAsync(string userId, string? ipAddress);
    }
}
