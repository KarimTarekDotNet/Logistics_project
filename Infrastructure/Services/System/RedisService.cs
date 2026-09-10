using Application.Interfaces.Services.System;
using Infrastructure.Common;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Services.System
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisService> _logger;

        public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task<(bool Success, T? Value)> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                    return (false, default);

                return (true, GenericSerializer.Deserialize<T>(value.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting value from Redis for key: {Key}", key);
                return (false, default);
            }
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task<(bool Success, bool Value)> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var json = GenericSerializer.Serialize(value);
                var expiration = expiry ?? TimeSpan.FromHours(1);
                var result = await _database.StringSetAsync(key, json, expiration);
                return (true, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while setting value in Redis for key: {Key}", key);
                return (false, false);
            }
        }

        public async Task<(bool Success, bool Acquired)> TryAcquireIdempotencyKeyAsync(string key, string paymentTransactionId, TimeSpan expiration)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Redis key is required.", nameof(key));

                if(string.IsNullOrWhiteSpace(paymentTransactionId))
                    throw new ArgumentException("Payment transaction ID is required.", nameof(paymentTransactionId));

                var json = GenericSerializer.Serialize(paymentTransactionId);

                var result = await _database.StringSetAsync(key, json, expiration, When.NotExists);
                return (true, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while trying to acquire idempotency key in Redis for key: {Key}", key);
                return (false, false);
            }
        }
    }
}