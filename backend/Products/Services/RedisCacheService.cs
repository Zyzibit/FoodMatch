using StackExchange.Redis;
using System.Text.Json;

namespace inzynierka.Products.Services;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _database;
    private readonly IConnectionMultiplexer _redis;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        if (!value.HasValue)
            return default;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, serializedValue, expiration);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern);
        
        foreach (var key in keys)
        {
            await _database.KeyDeleteAsync(key);
        }
    }

    public async Task<HashSet<string>> GetSetAsync(string key)
    {
        var values = await _database.SetMembersAsync(key);
        return new HashSet<string>(values.Select(v => v.ToString()));
    }

    public async Task AddToSetAsync(string key, string value)
    {
        await _database.SetAddAsync(key, value);
    }

    public async Task AddToSetAsync(string key, IEnumerable<string> values)
    {
        var redisValues = values.Select(v => (RedisValue)v).ToArray();
        await _database.SetAddAsync(key, redisValues);
    }
}