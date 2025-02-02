using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

public static class RabbitMqConfig
{
    public static void AddRabbitMqConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"],
                DispatchConsumersAsync = true // Включить асинхронных потребителей
            };
            return factory;
        });

        services.AddSingleton(sp =>
        {
            var factory = sp.GetRequiredService<ConnectionFactory>();
            return factory.CreateConnection();
        });

        services.AddSingleton(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();
            return connection.CreateModel();
        });

        services.AddSingleton<IConsumerInitializer, ConsumerInitializer>();
        services.AddSingleton<IRabbitMQInitializer, RabbitMQInitializer>();
    }

    public static void InitializeRabbitMq(IApplicationBuilder app)
    {
        var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
        using (var scope = scopeFactory.CreateScope())
        {
            var rabbitMQInitializer = scope.ServiceProvider.GetRequiredService<IRabbitMQInitializer>();
            rabbitMQInitializer.Initialize();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Startup>>();

            // Запуск прослушивания команд
            var eventHandlers = scope.ServiceProvider.GetServices<IEventHandler>();
            foreach (var handler in eventHandlers)
            {
                logger.LogInformation("Initializing StartListening for handler: {HandlerType}", handler.GetType().Name);
                handler.StartListening();
                logger.LogInformation("Started listening on queues for handler: {HandlerType}", handler.GetType().Name);
            }
        }
    }
}
