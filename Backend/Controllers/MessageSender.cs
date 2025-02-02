using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

public class MessageSender : IMessageSender
{
    private readonly IModel _channel;
    private readonly ILogger<MessageSender> _logger;

    public MessageSender(IModel channel, ILogger<MessageSender> logger)
    {
        _channel = channel;
        _logger = logger;
    }

    public async Task<T> SendCommandAndGetResponse<T>(string exchange, string routingKey, object command)
    {
        _logger.LogInformation("Sending command: {Command}", JsonConvert.SerializeObject(command));

        var tcs = new TaskCompletionSource<string>();
        var replyQueueName = _channel.QueueDeclare().QueueName;
        var correlationId = Guid.NewGuid().ToString();

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            _logger.LogInformation("Received response: {Response}", Encoding.UTF8.GetString(ea.Body.ToArray()));
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                var response = Encoding.UTF8.GetString(ea.Body.ToArray());
                tcs.SetResult(response);
            }
            await Task.Yield();
        };
        _channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consumer);

        var props = _channel.CreateBasicProperties();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;

        var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
        _channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: props, body: messageBytes);

        _logger.LogInformation("Published message to exchange: {Exchange} with routing key: {RoutingKey}", exchange, routingKey);

        var responseMessage = await tcs.Task;
        var result = JsonConvert.DeserializeObject<T>(responseMessage);

        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize the response message.");
        }

        _logger.LogInformation("Deserialized response: {Result}", JsonConvert.SerializeObject(result));
        
        return result;
    }
}
