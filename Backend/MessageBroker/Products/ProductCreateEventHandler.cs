using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class ProductCommandHandler : IEventHandler
{
    private readonly IProductCreateService _productCreateService;
    private readonly IProcessedEventService _processedEventService;
    private readonly IModel _channel;
    private readonly IConsumerInitializer _consumerInitializer;
    private readonly ICommandHandler _commandHandler;

    public ProductCommandHandler(
        IProductCreateService productCreateService,
        IProcessedEventService processedEventService,
        IModel channel,
        IConsumerInitializer consumerInitializer,
        ICommandHandler commandHandler)
    {
        _productCreateService = productCreateService;
        _processedEventService = processedEventService;
        _channel = channel;
        _consumerInitializer = consumerInitializer;
        _commandHandler = commandHandler;
    }

    public void StartListening()
    {
        _consumerInitializer.InitializeConsumer(_channel, "products.create", HandleCreateProductCommand);
        _consumerInitializer.InitializeConsumer(_channel, "products.uploadimage", HandleUploadImageCommand);
    }

    public async Task HandleCreateProductCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var createProductCommand = JsonConvert.DeserializeObject<CreateProductCommand>(dataString);
        if (createProductCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(createProductCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _productCreateService.AddProduct(createProductCommand.Product);
                await _processedEventService.MarkEventAsProcessed(createProductCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleUploadImageCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var uploadImageCommand = JsonConvert.DeserializeObject<UploadImageCommand>(dataString);
        if (uploadImageCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(uploadImageCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _productCreateService.UploadImage(uploadImageCommand.Id, uploadImageCommand.File);
                await _processedEventService.MarkEventAsProcessed(uploadImageCommand.CommandId);
                return result;
            });
        }
    }
}
