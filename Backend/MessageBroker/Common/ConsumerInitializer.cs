using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

public class ConsumerInitializer : IConsumerInitializer
{
    private readonly ILogger<ConsumerInitializer> _logger;

    public ConsumerInitializer(ILogger<ConsumerInitializer> logger)
    {
        _logger = logger;
    }

    public void InitializeConsumer(IModel channel, string queueName, Func<BasicDeliverEventArgs, Task> handleCommand)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            _logger.LogInformation("Message received on queue: {QueueName}", queueName);
            await handleCommand(ea);
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        _logger.LogInformation("Initialized consumer on queue: {QueueName}", queueName);
    }
}
