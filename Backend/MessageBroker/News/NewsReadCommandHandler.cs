using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class NewsReadCommandHandler : IEventHandler
{
    private readonly IReadNewsService _readNewsService;
    private readonly IProcessedEventService _processedEventService;
    private readonly IModel _channel;
    private readonly IConsumerInitializer _consumerInitializer;
    private readonly ICommandHandler _commandHandler;

    public NewsReadCommandHandler(
        IReadNewsService readNewsService,
        IProcessedEventService processedEventService,
        IModel channel,
        IConsumerInitializer consumerInitializer,
        ICommandHandler commandHandler)
    {
        _readNewsService = readNewsService;
        _processedEventService = processedEventService;
        _channel = channel;
        _consumerInitializer = consumerInitializer;
        _commandHandler = commandHandler;
    }

    public void StartListening()
    {
        _consumerInitializer.InitializeConsumer(_channel, "news.getall", HandleGetAllNewsQuery);
        _consumerInitializer.InitializeConsumer(_channel, "news.getbyid", HandleGetNewsByIdQuery);
    }

    public async Task HandleGetAllNewsQuery(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var getAllNewsQuery = JsonConvert.DeserializeObject<GetAllNewsQuery>(dataString);
        if (getAllNewsQuery != null)
        {
            if (await _processedEventService.IsEventProcessed(getAllNewsQuery.QueryId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _readNewsService.GetAllNews(getAllNewsQuery.PageNumber, getAllNewsQuery.PageSize);
                await _processedEventService.MarkEventAsProcessed(getAllNewsQuery.QueryId);
                return result;
            });
        }
    }

    public async Task HandleGetNewsByIdQuery(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var getNewsByIdQuery = JsonConvert.DeserializeObject<GetNewsByIdQuery>(dataString);
        if (getNewsByIdQuery != null)
        {
            if (await _processedEventService.IsEventProcessed(getNewsByIdQuery.QueryId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _readNewsService.GetNewsById(getNewsByIdQuery.NewsId);
                await _processedEventService.MarkEventAsProcessed(getNewsByIdQuery.QueryId);
                return result;
            });
        }
    }
}
