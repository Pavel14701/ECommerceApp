using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

public interface ICommandHandler
{
    Task HandleCommandAsync<T>(BasicDeliverEventArgs ea, Func<Task<T>>  handleFunc, bool shouldCache);
}


public class CommandHandler : ICommandHandler
{
    private readonly IModel _channel;
    private readonly ILogger<CommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public CommandHandler(IModel channel, ILogger<CommandHandler> logger, ICacheService cacheService)
    {
        _channel = channel;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task HandleCommandAsync<T>(BasicDeliverEventArgs ea, Func<Task<T>> handleFunc, bool shouldCache)
    {
        string cacheKey = $"response:{ea.BasicProperties.CorrelationId}";
        var result = await _cacheService.GetOrSetCacheAsync(cacheKey, handleFunc, TimeSpan.FromMinutes(10), shouldCache);

        var responseProps = _channel.CreateBasicProperties();
        responseProps.CorrelationId = ea.BasicProperties.CorrelationId;

        var responseMessage = JsonConvert.SerializeObject(result);
        _channel.BasicPublish(
            exchange: "",
            routingKey: ea.BasicProperties.ReplyTo,
            basicProperties: responseProps,
            body: Encoding.UTF8.GetBytes(responseMessage)
        );
    }
}
