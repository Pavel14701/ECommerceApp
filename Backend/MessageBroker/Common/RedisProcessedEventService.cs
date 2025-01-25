using StackExchange.Redis;

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
        return await db.KeyExistsAsync(eventId.ToString());
    }

    public async Task MarkEventAsProcessed(Guid eventId)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(eventId.ToString(), DateTime.UtcNow.ToString(), TimeSpan.FromDays(1)); // Устанавливаем срок хранения ключа, например, 1 день
    }
}
