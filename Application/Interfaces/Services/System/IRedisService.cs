namespace Application.Interfaces.Services.System
{
    public interface IRedisService
    {
        Task<bool> TryAcquireIdempotencyKeyAsync(string key, string paymentTransactionId, TimeSpan expiration);
        Task<bool> ExistsAsync(string key);
        Task RemoveAsync(string key);
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T?> GetAsync<T>(string key);
    }
}