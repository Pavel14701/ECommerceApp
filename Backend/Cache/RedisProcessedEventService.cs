using StackExchange.Redis;

public interface IProcessedEventService
{
    Task<bool> IsEventProcessed(Guid eventId);
    Task MarkEventAsProcessed(Guid eventId);
}

public class RedisProcessedEventService : IProcessedEventService
{
    private readonly IDatabase _redisDatabase;

    public RedisProcessedEventService(IConnectionMultiplexer redisConnection)
    {
        _redisDatabase = redisConnection.GetDatabase();
    }

    public async Task<bool> IsEventProcessed(Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Invalid event ID");
        }

        var exists = await _redisDatabase.KeyExistsAsync(eventId.ToString());
        return exists;
    }

    public async Task MarkEventAsProcessed(Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Invalid event ID");
        }

        await _redisDatabase.StringSetAsync(eventId.ToString(), DateTime.UtcNow.ToString(), TimeSpan.FromDays(1));
    }
}