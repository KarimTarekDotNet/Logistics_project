namespace Application.Interfaces.Services.System
{
    public interface IRedisService
    {
        Task<(bool Success, bool Acquired)> TryAcquireIdempotencyKeyAsync(string key, string paymentTransactionId, TimeSpan expiration);
        Task<bool> ExistsAsync(string key);
        Task RemoveAsync(string key);
        Task<(bool Success, bool Value)> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<(bool Success, T? Value)> GetAsync<T>(string key);
    }
}