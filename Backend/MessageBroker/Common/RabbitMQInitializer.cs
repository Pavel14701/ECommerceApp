using RabbitMQ.Client;

public class RabbitMQInitializer : IRabbitMQInitializer
{
    private readonly IModel _channel;

    public RabbitMQInitializer(IModel channel)
    {
        _channel = channel;
    }

    public void Initialize()
    {
        _channel.ExchangeDeclare(exchange: "auth.exchange", type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "auth.authenticate", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: "auth.authenticate", exchange: "auth.exchange", routingKey: "auth.authenticate");

        _channel.QueueDeclare(queue: "auth.refreshToken", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: "auth.refreshToken", exchange: "auth.exchange", routingKey: "auth.refreshToken");
    }
}
