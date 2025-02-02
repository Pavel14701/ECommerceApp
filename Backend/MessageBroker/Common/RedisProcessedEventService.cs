using StackExchange.Redis;
using Microsoft.Extensions.Logging;

public class RedisProcessedEventService : IProcessedEventService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisProcessedEventService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> IsEventProcessed(Guid eventId)
    {
        var db = _redis.GetDatabase();
        var exists = await db.KeyExistsAsync(eventId.ToString());
        return exists;
    }

    public async Task MarkEventAsProcessed(Guid eventId)
    {
        var db = _redis.GetDatabase();
        bool setSuccess = await db.StringSetAsync(eventId.ToString(), DateTime.UtcNow.ToString(), TimeSpan.FromDays(1));
    }
}
