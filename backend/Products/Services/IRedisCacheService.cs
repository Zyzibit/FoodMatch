namespace inzynierka.Products.Services;

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<bool> ExistsAsync(string key);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<HashSet<string>> GetSetAsync(string key);
    Task AddToSetAsync(string key, string value);
    Task AddToSetAsync(string key, IEnumerable<string> values);
}