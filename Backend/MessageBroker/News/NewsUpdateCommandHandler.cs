using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class NewsUpdateCommandHandler : IEventHandler
{
    private readonly IUpdateNewsService _updateNewsService;
    private readonly IConsumerInitializer _consumerInitializer;
    private readonly IProcessedEventService _processedEventService;
    private readonly ICommandHandler _commandHandler;
    private readonly IModel _channel;

    public NewsUpdateCommandHandler(
        IUpdateNewsService updateNewsService,
        IProcessedEventService processedEventService,
        IConsumerInitializer consumerInitializer,
        IModel channel,
        ICommandHandler commandHandler)
    {
        _updateNewsService = updateNewsService;
        _channel = channel;
        _consumerInitializer = consumerInitializer;
        _processedEventService = processedEventService;
        _commandHandler = commandHandler;
    }

    public void StartListening()
    {
        _consumerInitializer.InitializeConsumer(_channel, "news.update", HandleUpdateNewsCommand);
        _consumerInitializer.InitializeConsumer(_channel, "news.update.image", HandleUpdateImageCommand);
        _consumerInitializer.InitializeConsumer(_channel, "news.update.title", HandleUpdateNewsTitleCommand);
        _consumerInitializer.InitializeConsumer(_channel, "news.update.publishdate", HandleUpdateNewsPublishDateCommand);
        _consumerInitializer.InitializeConsumer(_channel, "news.update.contenttext", HandleUpdateNewsContentTextCommand);
    }

    public async Task HandleUpdateNewsCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updateNewsCommand = JsonConvert.DeserializeObject<UpdateNewsCommand>(dataString);
        if (updateNewsCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updateNewsCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _updateNewsService.UpdateNews(updateNewsCommand.News);
                await _processedEventService.MarkEventAsProcessed(updateNewsCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleUpdateImageCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updateImageCommand = JsonConvert.DeserializeObject<UpdateImageCommand>(dataString);
        if (updateImageCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updateImageCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _updateNewsService.UpdateImage(updateImageCommand.NewsId, updateImageCommand.ImageId, updateImageCommand.File);
                await _processedEventService.MarkEventAsProcessed(updateImageCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleUpdateNewsTitleCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updateNewsTitleCommand = JsonConvert.DeserializeObject<UpdateNewsTitleCommand>(dataString);
        if (updateNewsTitleCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updateNewsTitleCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _updateNewsService.UpdateNewsTitle(updateNewsTitleCommand.NewsId, updateNewsTitleCommand.Title);
                await _processedEventService.MarkEventAsProcessed(updateNewsTitleCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleUpdateNewsPublishDateCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updateNewsPublishDateCommand = JsonConvert.DeserializeObject<UpdateNewsPublishDateCommand>(dataString);
        if (updateNewsPublishDateCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updateNewsPublishDateCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _updateNewsService.UpdateNewsPublishDate(updateNewsPublishDateCommand.NewsId, updateNewsPublishDateCommand.PublishDate);
                await _processedEventService.MarkEventAsProcessed(updateNewsPublishDateCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleUpdateNewsContentTextCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updateNewsContentTextCommand = JsonConvert.DeserializeObject<UpdateNewsContentTextCommand>(dataString);
        if (updateNewsContentTextCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updateNewsContentTextCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                var result = await _updateNewsService.UpdateNewsContentText(updateNewsContentTextCommand.NewsId, updateNewsContentTextCommand.ContentId, updateNewsContentTextCommand.Text);
                await _processedEventService.MarkEventAsProcessed(updateNewsContentTextCommand.CommandId);
                return result;
            });
        }
    }
}
