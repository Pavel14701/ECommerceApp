using StackExchange.Redis;
using Microsoft.Extensions.Logging;

public class RedisProcessedEventService : IProcessedEventService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisProcessedEventService> _logger;

    public RedisProcessedEventService(IConnectionMultiplexer redis, ILogger<RedisProcessedEventService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> IsEventProcessed(Guid eventId)
{
    var db = _redis.GetDatabase();
    _logger.LogInformation("Checking if event {EventId} is processed", eventId);

    try
    {
        var exists = await db.KeyExistsAsync(eventId.ToString());
        _logger.LogInformation("Event {EventId} processed status: {Exists}", eventId, exists);
        return exists;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error checking if event {EventId} is processed", eventId);
        throw; // Проброс исключения для дальнейшей обработки
    }
}


    public async Task MarkEventAsProcessed(Guid eventId)
    {
        var db = _redis.GetDatabase();
        _logger.LogInformation("Attempting to mark event {EventId} as processed", eventId);
        bool setSuccess = await db.StringSetAsync(eventId.ToString(), DateTime.UtcNow.ToString(), TimeSpan.FromDays(1)); // Устанавливаем срок хранения ключа, например, 1 день
        _logger.LogInformation("Result of marking event {EventId} as processed: {SetSuccess}", eventId, setSuccess);
        if (!setSuccess)
        {
            _logger.LogWarning("Failed to mark event {EventId} as processed", eventId);
        }
        _logger.LogInformation("Marked event {EventId} as processed", eventId);
    }
}
