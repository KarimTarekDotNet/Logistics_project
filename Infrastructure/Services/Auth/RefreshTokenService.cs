using Application.Interfaces.Services.Auth;
using Domain.Entities.Users;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Infrastructure.Services.Auth
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ApplicationDbContext _dbContext;

        public RefreshTokenService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(string RawToken, RefreshToken RefreshToken)> GenerateAsync(string userId, string? ipAddress)
        {
            var rawToken = GenerateRandomToken();
            var hashedToken = HashToken(rawToken);
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                HashedToken = hashedToken,
                ApplicationUserId = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(15), // Set expiration as needed
                CreatedByIp = ipAddress
            };
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            return (rawToken, refreshToken);
        }

        public async Task<RefreshToken?> GetByRawTokenAsync(string rawToken)
        {
            var hashedToken = HashToken(rawToken);
            var refreshToken = await _dbContext.RefreshTokens
                .Include(rt => rt.ApplicationUser)
                .FirstOrDefaultAsync(rt => rt.HashedToken == hashedToken);

            return refreshToken;
        }

        public async Task<List<RefreshToken>?> GetByListTokenUserIdAsync(string userId)
        {
            var refreshToken = _dbContext.RefreshTokens
                .Include(rt => rt.ApplicationUser)
                .Where(r => r.ApplicationUserId == userId && r.RevokedAt == null && r.ExpiresAt > DateTimeOffset.UtcNow)
                .ToList();

            return refreshToken;
        }

        public string HashToken(string rawToken)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToBase64String(bytes);
        }

        public async Task<bool> RevokeAsync(string userId, string rawToken, string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
                return false;

            var refreshToken = await GetByRawTokenAsync(rawToken!);
            if (refreshToken == null || refreshToken.ApplicationUserId != userId)
            {
                return false;
            }
            if (!refreshToken.IsActive)
                return false;

            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            refreshToken.RevokedByIp = ipAddress;

            return true;
        }

        public async Task<(string RawToken, RefreshToken NewRefreshToken)> RotateAsync(RefreshToken oldRefreshToken, string userId, string? ipAddress)
        {
            var newRawToken = GenerateRandomToken();
            var newHashedToken = HashToken(newRawToken);

            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                HashedToken = newHashedToken,
                ApplicationUserId = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(15),
                CreatedByIp = ipAddress
            };

            oldRefreshToken.RevokedAt = DateTimeOffset.UtcNow;
            oldRefreshToken.RevokedByIp = ipAddress;
            oldRefreshToken.ReplacedByHashedToken = newHashedToken;

            await _dbContext.RefreshTokens.AddAsync(newRefreshToken);
            return (newRawToken, newRefreshToken);
        }

        private string GenerateRandomToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
