using FluidCash.IExternalServicesRepo;
using StackExchange.Redis;
using System.Text.Json;

namespace FluidCash.ExternalServicesRepo;

public sealed class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _database;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    // Save any type of data to Redis with an expiration time
    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan expiry)
    {
        var jsonData = JsonSerializer.Serialize(value);
        return await _database.StringSetAsync(key, jsonData, expiry);
    }

    // Get cached data and deserialize it to the specified type
    public async Task<T?> GetAsync<T>(string key)
    {
        var jsonData = await _database.StringGetAsync(key);
        if (jsonData.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(jsonData!);
    }

    // Remove a key from Redis
    public async Task<bool> RemoveAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

    // Check if a key exists in Redis
    public async Task<bool> ExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }
}