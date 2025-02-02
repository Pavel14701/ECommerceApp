using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

public class AuthCommandHandler : IEventHandler
{
    private readonly IAuthService _authService;
    private readonly IProcessedEventService _processedEventService;
    private readonly IModel _channel;
    private readonly IConsumerInitializer _consumerInitializer;
    private readonly ICommandHandler _commandHandler;
    private readonly ILogger<AuthCommandHandler> _logger;

    public AuthCommandHandler(
        IAuthService authService,
        IProcessedEventService processedEventService,
        IModel channel,
        IConsumerInitializer consumerInitializer,
        ICommandHandler commandHandler,
        ILogger<AuthCommandHandler> logger)
    {
        _authService = authService;
        _processedEventService = processedEventService;
        _channel = channel;
        _consumerInitializer = consumerInitializer;
        _commandHandler = commandHandler;
        _logger = logger;
    }

    public void StartListening()
    {
        _consumerInitializer.InitializeConsumer(_channel, "auth.authenticate", HandleAuthenticateCommand);
        _consumerInitializer.InitializeConsumer(_channel, "auth.refreshToken", HandleRefreshTokenCommand);
        _logger.LogInformation("Started listening on auth.authenticate and auth.refreshToken queues");
    }

    public async Task HandleAuthenticateCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var authenticateCommand = JsonConvert.DeserializeObject<AuthenticateCommand>(dataString);
        _logger.LogInformation("Start HandleAuthenticateCommand: {Command}", dataString);

        if (authenticateCommand != null)
        {
            _logger.LogInformation("Processing authenticate command: {CommandId}", authenticateCommand.CommandId);
            
            _logger.LogInformation("Checking if event is processed: {CommandId}", authenticateCommand.CommandId);
            if (await _processedEventService.IsEventProcessed(authenticateCommand.CommandId))
            {
                _logger.LogInformation("Command already processed: {CommandId}", authenticateCommand.CommandId);
                return;
            }
            
            _logger.LogInformation("Event not processed, continue handling command: {CommandId}", authenticateCommand.CommandId);
            _logger.LogInformation("Start handling command async: {CommandId}", authenticateCommand.CommandId);

            await _commandHandler.HandleCommandAsync<AuthResultDto>(ea, async () =>
            {
                _logger.LogInformation("Calling AuthService.Authenticate for user: {Username}", authenticateCommand.Username);
                var result = await _authService.Authenticate(authenticateCommand.Username, authenticateCommand.Password);
                _logger.LogInformation("Authentication result: {Result}", JsonConvert.SerializeObject(result));
                await _processedEventService.MarkEventAsProcessed(authenticateCommand.CommandId);
                _logger.LogInformation("Command marked as processed: {CommandId}", authenticateCommand.CommandId);
                return result;
            });
            
            _logger.LogInformation("End handling command async: {CommandId}", authenticateCommand.CommandId);
        }
        else
        {
            _logger.LogWarning("Failed to deserialize authenticate command: {Data}", dataString);
        }

        _logger.LogInformation("End HandleAuthenticateCommand: {CommandId}", authenticateCommand?.CommandId);
    }



    public async Task HandleRefreshTokenCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var refreshTokenCommand = JsonConvert.DeserializeObject<RefreshTokenCommand>(dataString);
        _logger.LogInformation("Start HandleRefreshTokenCommand: {Command}", dataString);

        if (refreshTokenCommand != null)
        {
            _logger.LogInformation("Processing refresh token command: {CommandId}", refreshTokenCommand.CommandId);
            if (await _processedEventService.IsEventProcessed(refreshTokenCommand.CommandId))
            {
                _logger.LogInformation("Command already processed: {CommandId}", refreshTokenCommand.CommandId);
                return;
            }

            await _commandHandler.HandleCommandAsync<AuthResultDto>(ea, async () =>
            {
                _logger.LogInformation("Calling AuthService.RefreshToken for token: {RefreshToken}", refreshTokenCommand.RefreshToken);
                var result = await _authService.RefreshToken(refreshTokenCommand.RefreshToken);
                _logger.LogInformation("Refresh token result: {Result}", JsonConvert.SerializeObject(result));
                await _processedEventService.MarkEventAsProcessed(refreshTokenCommand.CommandId);
                return result;
            });
        }
        else
        {
            _logger.LogWarning("Failed to deserialize refresh token command: {Data}", dataString);
        }

        _logger.LogInformation("End HandleRefreshTokenCommand: {CommandId}", refreshTokenCommand?.CommandId);
    }
}
