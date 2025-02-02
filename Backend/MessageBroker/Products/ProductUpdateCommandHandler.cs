using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class ProductUpdateCommandHandler : IEventHandler
{
    private readonly IProductUpdateService _productUpdateService;
    private readonly IProcessedEventService _processedEventService;
    private readonly IModel _channel;
    private readonly IConsumerInitializer _consumerInitializer;
    private readonly ICommandHandler _commandHandler;

    public ProductUpdateCommandHandler(
        IProductUpdateService productUpdateService,
        IProcessedEventService processedEventService,
        IModel channel,
        IConsumerInitializer consumerInitializer,
        ICommandHandler commandHandler)
    {
        _productUpdateService = productUpdateService;
        _processedEventService = processedEventService;
        _channel = channel;
        _consumerInitializer = consumerInitializer;
        _commandHandler = commandHandler;
    }

    public void StartListening()
    {
        _consumerInitializer.InitializeConsumer(_channel, "products.update.name", HandleUpdateProductNameCommand);
        _consumerInitializer.InitializeConsumer(_channel, "products.update.category", HandleUpdateProductCategoryCommand);
        _consumerInitializer.InitializeConsumer(_channel, "products.update.price", HandleUpdateProductPriceCommand);
        _consumerInitializer.InitializeConsumer(_channel, "products.update.stock", HandleUpdateProductStockCommand);
        _consumerInitializer.InitializeConsumer(_channel, "products.update.description", HandleUpdateProductDescriptionCommand);
        _consumerInitializer.InitializeConsumer(_channel, "products.update.image", HandleUpdateProductImageCommand);
        _consumerInitializer.InitializeConsumer(_channel, "products.update.product", HandleUpdateProductCommand);
    }

    public async Task HandleUpdateProductNameCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updateProductNameCommand = JsonConvert.DeserializeObject<UpdateProductNameCommand>(dataString);
        if (updateProductNameCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updateProductNameCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _productUpdateService.UpdateProductName(updateProductNameCommand.ProductId, updateProductNameCommand.Name);
                await _processedEventService.MarkEventAsProcessed(updateProductNameCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleUpdateProductCategoryCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updateProductCategoryCommand = JsonConvert.DeserializeObject<UpdateProductCategoryCommand>(dataString);
        if (updateProductCategoryCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updateProductCategoryCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _productUpdateService.UpdateProductCategory(updateProductCategoryCommand.ProductId, updateProductCategoryCommand.Category);
                await _processedEventService.MarkEventAsProcessed(updateProductCategoryCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleUpdateProductPriceCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updateProductPriceCommand = JsonConvert.DeserializeObject<UpdateProductPriceCommand>(dataString);
        if (updateProductPriceCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updateProductPriceCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _productUpdateService.UpdateProductPrice(updateProductPriceCommand.ProductId, updateProductPriceCommand.Price);
                await _processedEventService.MarkEventAsProcessed(updateProductPriceCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleUpdateProductStockCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updateProductStockCommand = JsonConvert.DeserializeObject<UpdateProductStockCommand>(dataString);
        if (updateProductStockCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updateProductStockCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _productUpdateService.UpdateProductStock(updateProductStockCommand.ProductId, updateProductStockCommand.Stock);
                await _processedEventService.MarkEventAsProcessed(updateProductStockCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleUpdateProductDescriptionCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updateProductDescriptionCommand = JsonConvert.DeserializeObject<UpdateProductDescriptionCommand>(dataString);
        if (updateProductDescriptionCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updateProductDescriptionCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _productUpdateService.UpdateProductDescription(updateProductDescriptionCommand.ProductId, updateProductDescriptionCommand.Description);
                await _processedEventService.MarkEventAsProcessed(updateProductDescriptionCommand.CommandId);
                return result;
            });
        }   
    }

    public async Task HandleUpdateProductImageCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updateProductImageCommand = JsonConvert.DeserializeObject<UpdateProductImageCommand>(dataString);
        if (updateProductImageCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updateProductImageCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _productUpdateService.UpdateProductImage(updateProductImageCommand.ProductId, updateProductImageCommand.ImageId, updateProductImageCommand.File);
                await _processedEventService.MarkEventAsProcessed(updateProductImageCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleUpdateProductCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updateProductCommand = JsonConvert.DeserializeObject<UpdateProductCommand>(dataString);
        if (updateProductCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updateProductCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _productUpdateService.UpdateProduct(updateProductCommand.Product);
                await _processedEventService.MarkEventAsProcessed(updateProductCommand.CommandId);
                return result;
            });
        }
    }
}
