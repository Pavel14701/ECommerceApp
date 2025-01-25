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

public interface IProcessedEventService
{
    Task<bool> IsEventProcessed(Guid eventId);
    Task MarkEventAsProcessed(Guid eventId);
}

public interface ICommandHandler
{
    Task HandleCommandAsync(BasicDeliverEventArgs ea, Func<Task> handleFunc);
}
