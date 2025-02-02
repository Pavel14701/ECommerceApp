using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

public class ConsumerInitializer : IConsumerInitializer
{ public void InitializeConsumer(IModel channel, string queueName, Func<BasicDeliverEventArgs, Task> handleCommand)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            await handleCommand(ea);
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }
}
