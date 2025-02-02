using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class NewsDeleteCommandHandler : IEventHandler
{
    private readonly IDeleteNewsService _deleteNewsService;
    private readonly IProcessedEventService _processedEventService;
    private readonly IModel _channel;
    private readonly IConsumerInitializer _consumerInitializer;
    private readonly ICommandHandler _commandHandler;

    public NewsDeleteCommandHandler(
        IDeleteNewsService deleteNewsService,
        IProcessedEventService processedEventService,
        IModel channel,
        IConsumerInitializer consumerInitializer,
        ICommandHandler commandHandler)
    {
        _deleteNewsService = deleteNewsService;
        _processedEventService = processedEventService;
        _channel = channel;
        _consumerInitializer = consumerInitializer;
        _commandHandler = commandHandler;
    }

    public void StartListening()
    {
        _consumerInitializer.InitializeConsumer(_channel, "news.delete", HandleDeleteNewsCommand);
        _consumerInitializer.InitializeConsumer(_channel, "news.delete.image", HandleDeleteImageCommand);
    }

    public async Task HandleDeleteNewsCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var deleteNewsCommand = JsonConvert.DeserializeObject<DeleteNewsCommand>(dataString);
        if (deleteNewsCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(deleteNewsCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _deleteNewsService.DeleteNews(deleteNewsCommand.NewsId);
                await _processedEventService.MarkEventAsProcessed(deleteNewsCommand.CommandId);
                return result;
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
                var result = await _deleteNewsService.DeleteImage(deleteImageCommand.ObjectId, deleteImageCommand.ImageId);
                await _processedEventService.MarkEventAsProcessed(deleteImageCommand.CommandId);
                return result;
            });
        }
    }

}
