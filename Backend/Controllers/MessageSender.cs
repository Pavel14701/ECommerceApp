using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class MessageSender : IMessageSender
{
    private readonly IModel _channel;

    public MessageSender(IModel channel)
    {
        _channel = channel;
    }

    public async Task<T> SendCommandAndGetResponse<T>(string exchange, string routingKey, object command)
    {
        var tcs = new TaskCompletionSource<string>();
        var replyQueueName = _channel.QueueDeclare().QueueName;
        var correlationId = Guid.NewGuid().ToString();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                var response = Encoding.UTF8.GetString(ea.Body.ToArray());
                tcs.SetResult(response);
            }
        };
        _channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consumer);

        var props = _channel.CreateBasicProperties();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;

        var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
        _channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: props, body: messageBytes);

        var responseMessage = await tcs.Task;
        var result = JsonConvert.DeserializeObject<T>(responseMessage);

        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize the response message.");
        }

        return result;
    }
}
