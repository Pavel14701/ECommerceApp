using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class ProductQueryHandler : IEventHandler
{
    private readonly IProductReadService _productReadService;
    private readonly IProcessedEventService _processedEventService;
    private readonly IModel _channel;
    private readonly IConsumerInitializer _consumerInitializer;
    private readonly ICommandHandler _commandHandler;

    public ProductQueryHandler(
        IProductReadService productReadService,
        IProcessedEventService processedEventService,
        IModel channel,
        IConsumerInitializer consumerInitializer,
        ICommandHandler commandHandler)
    {
        _productReadService = productReadService;
        _processedEventService = processedEventService;
        _channel = channel;
        _consumerInitializer = consumerInitializer;
        _commandHandler = commandHandler;
    }

    public void StartListening()
    {
        _consumerInitializer.InitializeConsumer(_channel, "products.getall", HandleGetAllProductsQuery);
        _consumerInitializer.InitializeConsumer(_channel, "products.getbyid", HandleGetProductByIdQuery);
        _consumerInitializer.InitializeConsumer(_channel, "products.getbycategory", HandleGetProductsByCategoryQuery);
        _consumerInitializer.InitializeConsumer(_channel, "products.getbyname", HandleGetProductsByNameQuery);
    }

    public async Task HandleGetAllProductsQuery(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var getAllProductsQuery = JsonConvert.DeserializeObject<GetAllProductsQuery>(dataString);
        if (getAllProductsQuery != null)
        {
            if (await _processedEventService.IsEventProcessed(getAllProductsQuery.QueryId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var products = await _productReadService.GetAllProducts(getAllProductsQuery.PageNumber, getAllProductsQuery.PageSize);
                await _processedEventService.MarkEventAsProcessed(getAllProductsQuery.QueryId);
            });
        }
    }

    public async Task HandleGetProductByIdQuery(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var getProductByIdQuery = JsonConvert.DeserializeObject<GetProductByIdQuery>(dataString);
        if (getProductByIdQuery != null)
        {
            if (await _processedEventService.IsEventProcessed(getProductByIdQuery.QueryId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var product = await _productReadService.GetProductById(getProductByIdQuery.ProductId);
                await _processedEventService.MarkEventAsProcessed(getProductByIdQuery.QueryId);
            });
        }
    }

    public async Task HandleGetProductsByCategoryQuery(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var getProductsByCategoryQuery = JsonConvert.DeserializeObject<GetProductsByCategoryQuery>(dataString);
        if (getProductsByCategoryQuery != null)
        {
            if (await _processedEventService.IsEventProcessed(getProductsByCategoryQuery.QueryId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var products = await _productReadService.GetProductsByCategory(getProductsByCategoryQuery.Category, getProductsByCategoryQuery.PageNumber, getProductsByCategoryQuery.PageSize);
                await _processedEventService.MarkEventAsProcessed(getProductsByCategoryQuery.QueryId);
            });
        }
    }

    public async Task HandleGetProductsByNameQuery(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var getProductsByNameQuery = JsonConvert.DeserializeObject<GetProductsByNameQuery>(dataString);
        if (getProductsByNameQuery != null)
        {
            if (await _processedEventService.IsEventProcessed(getProductsByNameQuery.QueryId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var products = await _productReadService.GetProductsByName(getProductsByNameQuery.Name, getProductsByNameQuery.PageNumber, getProductsByNameQuery.PageSize);
                await _processedEventService.MarkEventAsProcessed(getProductsByNameQuery.QueryId);
            });
        }
    }
}
