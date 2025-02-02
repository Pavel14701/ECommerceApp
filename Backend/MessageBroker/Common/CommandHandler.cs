using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

public class CommandHandler : ICommandHandler
{
    private readonly IModel _channel;
    private readonly ILogger<CommandHandler> _logger;

    public CommandHandler(IModel channel, ILogger<CommandHandler> logger)
    {
        _channel = channel;
        _logger = logger;
    }

    public async Task HandleCommandAsync<T>(BasicDeliverEventArgs ea, Func<Task<T>> handleFunc)
    {
        var result = await handleFunc();
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
