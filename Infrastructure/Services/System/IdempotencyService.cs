using Application.Interfaces.Services.System;

namespace Infrastructure.Services.System
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly IRedisService _redis;

        public IdempotencyService(IRedisService redis)
        {
            _redis = redis;
        }

        public Task<string?> GetExisting(string key)
        {
            try
            {
                return _redis.GetAsync<string>(key);
            }
            catch
            {
                // Redis failed → system must NOT fail
                return null!;
            }
        }

        public async Task<bool> TryStartOperation(string key, string value, TimeSpan ttl)
        {
            try
            {
                return await _redis.TryAcquireIdempotencyKeyAsync(key, value, ttl);
            }
            catch
            {
                // Redis failed → system must NOT fail
                return false;
            }
        }
    }
}
