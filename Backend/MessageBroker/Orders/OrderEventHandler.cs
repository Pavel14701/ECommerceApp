using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class OrderHandler : IEventHandler
{
    private readonly IOrderService _orderService;
    private readonly IConsumerInitializer _consumerInitializer;
    private readonly IProcessedEventService _processedEventService;
    private readonly ICommandHandler _commandHandler;
    private readonly IModel _channel;

    public OrderHandler(
        IOrderService orderService,
        IProcessedEventService processedEventService,
        IConsumerInitializer consumerInitializer,
        IModel channel,
        ICommandHandler commandHandler)
    {
        _orderService = orderService;
        _channel = channel;
        _consumerInitializer = consumerInitializer;
        _processedEventService = processedEventService;
        _commandHandler = commandHandler;
    }

    public void StartListening()
    {
        _consumerInitializer.InitializeConsumer(_channel, "orders.create", HandleCreateOrderCommand);
        _consumerInitializer.InitializeConsumer(_channel, "orders.applyDiscount", HandleApplyDiscountCommand);
        _consumerInitializer.InitializeConsumer(_channel, "orders.getById", HandleGetOrderByIdQuery);
    }

    public async Task HandleCreateOrderCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var createOrderCommand = JsonConvert.DeserializeObject<CreateOrderCommand>(dataString);
        if (createOrderCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(createOrderCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _orderService.CreateOrder(createOrderCommand.Order);
                await _processedEventService.MarkEventAsProcessed(createOrderCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleApplyDiscountCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var applyDiscountCommand = JsonConvert.DeserializeObject<ApplyDiscountCommand>(dataString);
        if (applyDiscountCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(applyDiscountCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _orderService.ApplyDiscount(applyDiscountCommand.OrderId, applyDiscountCommand.Discount);
                await _processedEventService.MarkEventAsProcessed(applyDiscountCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleGetOrderByIdQuery(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var getOrderByIdQuery = JsonConvert.DeserializeObject<GetOrderByIdQuery>(dataString);
        if (getOrderByIdQuery != null)
        {
            if (await _processedEventService.IsEventProcessed(getOrderByIdQuery.QueryId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _orderService.GetOrderById(getOrderByIdQuery.OrderId);
                await _processedEventService.MarkEventAsProcessed(getOrderByIdQuery.QueryId);
                return result;
            });
        }
    }
}
