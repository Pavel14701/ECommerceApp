using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class CommandHandler : ICommandHandler
{
    private readonly IModel _channel;
    public CommandHandler(IModel channel)
    {
        _channel = channel;
    }

    public async Task HandleCommandAsync(BasicDeliverEventArgs ea, Func<Task> handleFunc)
    {
        await handleFunc();
        var responseProps = _channel.CreateBasicProperties();
        responseProps.CorrelationId = ea.BasicProperties.CorrelationId;

        var responseMessage = JsonConvert.SerializeObject("Command Processed");
        _channel.BasicPublish(
            exchange: "",
            routingKey: ea.BasicProperties.ReplyTo,
            basicProperties: responseProps,
            body: Encoding.UTF8.GetBytes(responseMessage)
        );
    }
}
