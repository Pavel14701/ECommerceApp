using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

public interface IConsumerInitializer
{
    void InitializeConsumer(IModel channel, string queueName, Func<BasicDeliverEventArgs, Task> handleCommand);
}

public interface IEventHandler
{
    void StartListening();
}


public interface IRabbitMQInitializer
{
    void Initialize();
}
