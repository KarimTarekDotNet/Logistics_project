using Domain.Entities.Users;

namespace Application.Interfaces.Services.Auth
{
    public interface IRefreshTokenService
    {
        Task<(string RawToken, RefreshToken RefreshToken)> GenerateAsync(string userId, string? ipAddress);
        Task<bool> RevokeAsync(string userId, string rawToken, string? ipAddress);
        string HashToken(string rawToken);
        Task<RefreshToken?> GetByRawTokenAsync(string rawToken);
        Task<List<RefreshToken>?> GetByListTokenUserIdAsync(string userId);
        Task<(string RawToken, RefreshToken NewRefreshToken)> RotateAsync(RefreshToken oldRefreshToken, string userId, string? ipAddress);
    }
}
