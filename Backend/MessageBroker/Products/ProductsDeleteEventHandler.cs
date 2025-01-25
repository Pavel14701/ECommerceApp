using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class ProductDeleteCommandHandler : IEventHandler
{
    private readonly IProductDeleteService _productDeleteService;
    private readonly IProcessedEventService _processedEventService;
    private readonly IModel _channel;
    private readonly IConsumerInitializer _consumerInitializer;
    private readonly ICommandHandler _commandHandler;

    public ProductDeleteCommandHandler(
        IProductDeleteService productDeleteService,
        IProcessedEventService processedEventService,
        IModel channel,
        IConsumerInitializer consumer_initializer,
        ICommandHandler command_handler)
    {
        _productDeleteService = productDeleteService;
        _processedEventService = processedEventService;
        _channel = channel;
        _consumerInitializer = consumer_initializer;
        _commandHandler = command_handler;
    }

    public void StartListening()
    {
        _consumerInitializer.InitializeConsumer(_channel, "products.delete", HandleDeleteProductCommand);
        _consumerInitializer.InitializeConsumer(_channel, "products.deleteimage", HandleDeleteImageCommand);
    }

    public async Task HandleDeleteProductCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var deleteProductCommand = JsonConvert.DeserializeObject<DeleteProductCommand>(dataString);
        if (deleteProductCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(deleteProductCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _productDeleteService.DeleteProduct(deleteProductCommand.ProductId);
                await _processedEventService.MarkEventAsProcessed(deleteProductCommand.CommandId);
            });
        }
    }

    public async Task HandleDeleteImageCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var deleteImageCommand = JsonConvert.DeserializeObject<DeleteImageCommand>(dataString);
        if (deleteImageCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(deleteImageCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _productDeleteService.DeleteImage(deleteImageCommand.ObjectId, deleteImageCommand.ImageId);
                await _processedEventService.MarkEventAsProcessed(deleteImageCommand.CommandId);
            });
        }
    }
}
