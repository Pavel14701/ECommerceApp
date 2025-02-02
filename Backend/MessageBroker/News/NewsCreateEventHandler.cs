using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class NewsEventHandler : IEventHandler
{
    private readonly ICreateNewsService _createNewsService;
    private readonly IProcessedEventService _processedEventService;
    private readonly IModel _channel;
    private readonly IConsumerInitializer _consumerInitializer;
    private readonly ICommandHandler _commandHandler;

    public NewsEventHandler(
        ICreateNewsService createNewsService,
        IProcessedEventService processedEventService,
        IModel channel,
        IConsumerInitializer consumerInitializer,
        ICommandHandler commandHandler)
    {
        _createNewsService = createNewsService;
        _processedEventService = processedEventService;
        _channel = channel;
        _consumerInitializer = consumerInitializer;
        _commandHandler = commandHandler;
    }

    public void StartListening()
    {
        _consumerInitializer.InitializeConsumer(_channel, "news.create", HandleCreateNewsCommand);
        _consumerInitializer.InitializeConsumer(_channel, "news.addimage", HandleAddImageToNewsCommand);
        _consumerInitializer.InitializeConsumer(_channel, "news.uploadimage", HandleUploadImageCommand);
    }

    public async Task HandleCreateNewsCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var createNewsCommand = JsonConvert.DeserializeObject<CreateNewsCommand>(dataString);
        if (createNewsCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(createNewsCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync<NewsCreationResultDto>(ea, async () =>
            {
                var result = await _createNewsService.AddNews(createNewsCommand.News);
                await _processedEventService.MarkEventAsProcessed(createNewsCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleAddImageToNewsCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var addImageToNewsCommand = JsonConvert.DeserializeObject<AddImageToNewsCommand>(dataString);
        if (addImageToNewsCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(addImageToNewsCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync<NewsCreationResultDto>(ea, async () =>
            {
                var result = await _createNewsService.AddImageToNews(addImageToNewsCommand.NewsId, addImageToNewsCommand.Image);
                await _processedEventService.MarkEventAsProcessed(addImageToNewsCommand.CommandId);
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

            await _commandHandler.HandleCommandAsync<ImageUploadResultDto>(ea, async () =>
            {
                var result = await _createNewsService.UploadImage(uploadImageCommand.Id, uploadImageCommand.File);
                await _processedEventService.MarkEventAsProcessed(uploadImageCommand.CommandId);
                return result;
            });
        }
    }
}
