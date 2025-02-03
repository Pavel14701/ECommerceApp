using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class AuthCommandHandler : IEventHandler
{
    private readonly IAuthService _authService;
    private readonly IProcessedEventService _processedEventService;
    private readonly IModel _channel;
    private readonly IConsumerInitializer _consumerInitializer;
    private readonly ICommandHandler _commandHandler;
    public AuthCommandHandler(
        IAuthService authService,
        IProcessedEventService processedEventService,
        IModel channel,
        IConsumerInitializer consumerInitializer,
        ICommandHandler commandHandler)
    {
        _authService = authService;
        _processedEventService = processedEventService;
        _channel = channel;
        _consumerInitializer = consumerInitializer;
        _commandHandler = commandHandler;
    }

    public void StartListening()
    {
        _consumerInitializer.InitializeConsumer(
            _channel, "auth.authenticate", HandleAuthenticateCommand
        );
        _consumerInitializer.InitializeConsumer(
            _channel, "auth.refreshToken", HandleRefreshTokenCommand
        );
    }

    public async Task HandleAuthenticateCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var authenticateCommand = JsonConvert.DeserializeObject<AuthenticateCommand>(dataString);
        if (authenticateCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(authenticateCommand.CommandId))
            {
                return;
            }
            await _commandHandler.HandleCommandAsync<AuthResultDto>(ea, async () =>
            {
                var _params = new AuthDtoParams
                {
                    Username = authenticateCommand.Username,
                    Password = authenticateCommand.Password
                };
                var result = await _authService.Authenticate(_params);
                await _processedEventService.MarkEventAsProcessed(authenticateCommand.CommandId);
                return result;
            });
        }
    }

    public async Task HandleRefreshTokenCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var refreshTokenCommand = JsonConvert.DeserializeObject<RefreshTokenCommand>(dataString);
        if (refreshTokenCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(refreshTokenCommand.CommandId))
            {
                return;
            }
            await _commandHandler.HandleCommandAsync<AuthResultDto>(ea, async () =>
            {
                var _params = new RefreshDtoParams
                {
                    RefreshToken = refreshTokenCommand.RefreshToken
                };
                var result = await _authService.RefreshToken(_params);
                await _processedEventService.MarkEventAsProcessed(refreshTokenCommand.CommandId);
                return result;
            });
        }
    }
}