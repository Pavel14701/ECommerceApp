using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class ConsumerInitializer : IConsumerInitializer
{
    public void InitializeConsumer(IModel channel, string queueName, Func<BasicDeliverEventArgs, Task> handleCommand)
    {
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            await handleCommand(ea);
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
    }
}
