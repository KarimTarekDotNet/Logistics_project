using Application.Interfaces.Services.System;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Services.System
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _database;

        public RedisService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value.ToString());
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            var expiration = expiry ?? TimeSpan.FromHours(1);
            return _database.StringSetAsync(key, json, expiration);
        }

        public async Task<bool> TryAcquireIdempotencyKeyAsync(string key, string paymentTransactionId, TimeSpan expiration)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Redis key is required.", nameof(key));

            if(string.IsNullOrWhiteSpace(paymentTransactionId))
                throw new ArgumentException("Payment transaction ID is required.", nameof(paymentTransactionId));

            var json = JsonSerializer.Serialize(paymentTransactionId);

            return await _database.StringSetAsync(key, json, expiration, When.NotExists);
        }
    }
}
