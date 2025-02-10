using StackExchange.Redis;
using Newtonsoft.Json;


public interface ICacheService
{
    Task<T?> GetOrSetCacheAsync<T>(string cacheKey, Func<Task<T>> getDataFunc, TimeSpan cacheDuration, bool shouldCache);
    Task<string?> GetCacheAsync(string cacheKey);
    Task SetCacheAsync(string cacheKey, string data, TimeSpan cacheDuration);
}


public class RedisCacheService : ICacheService
{
    private readonly IDatabase _redisDatabase;

    public RedisCacheService(IConnectionMultiplexer redisConnection)
    {
        _redisDatabase = redisConnection.GetDatabase();
    }

    public async Task<T?> GetOrSetCacheAsync<T>(string cacheKey, Func<Task<T>> getDataFunc, TimeSpan cacheDuration, bool shouldCache)
    {
        var cachedData = await _redisDatabase.StringGetAsync(cacheKey);
        if (!cachedData.IsNullOrEmpty)
        {
            string? cachedString = cachedData.ToString();
            if (!string.IsNullOrEmpty(cachedString))
            {
                var deserializedData = JsonConvert.DeserializeObject<T>(cachedString);
                if (deserializedData != null)
                {
                    return deserializedData;
                }
            }
        }
        var data = await getDataFunc();
        if (shouldCache)
        {
            var serializedData = JsonConvert.SerializeObject(data);
            await _redisDatabase.StringSetAsync(cacheKey, serializedData, cacheDuration);
        }

        return data;
    }

    public async Task<string?> GetCacheAsync(string cacheKey)
    {
        var cachedData = await _redisDatabase.StringGetAsync(cacheKey);
        return cachedData.IsNullOrEmpty ? null : cachedData.ToString();
    }

    public async Task SetCacheAsync(string cacheKey, string data, TimeSpan cacheDuration)
    {
        if (!string.IsNullOrEmpty(data))
        {
            await _redisDatabase.StringSetAsync(cacheKey, data, cacheDuration);
        }
    }
}
