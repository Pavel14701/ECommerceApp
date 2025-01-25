using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class UserCommandHandler : IEventHandler
{
    private readonly IUserService _userService;
    private readonly IProcessedEventService _processedEventService;
    private readonly IModel _channel;
    private readonly IConsumerInitializer _consumerInitializer;
    private readonly ICommandHandler _commandHandler;

    public UserCommandHandler(
        IUserService userService,
        IProcessedEventService processedEventService,
        IModel channel,
        IConsumerInitializer consumerInitializer,
        ICommandHandler commandHandler)
    {
        _userService = userService;
        _processedEventService = processedEventService;
        _channel = channel;
        _consumerInitializer = consumerInitializer;
        _commandHandler = commandHandler;
    }

    public void StartListening()
    {
        _consumerInitializer.InitializeConsumer(_channel, "users.register", HandleRegisterUserCommand);
        _consumerInitializer.InitializeConsumer(_channel, "users.confirmemail", HandleConfirmEmailCommand);
        _consumerInitializer.InitializeConsumer(_channel, "users.delete", HandleDeleteUserCommand);
        _consumerInitializer.InitializeConsumer(_channel, "users.updateusername", HandleUpdateUsernameCommand);
        _consumerInitializer.InitializeConsumer(_channel, "users.updatepassword", HandleUpdatePasswordCommand);
    }

    public async Task HandleRegisterUserCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var registerUserCommand = JsonConvert.DeserializeObject<RegisterUserCommand>(dataString);
        if (registerUserCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(registerUserCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                await _userService.RegisterUser(registerUserCommand.Username, registerUserCommand.Email, registerUserCommand.Password);
                await _processedEventService.MarkEventAsProcessed(registerUserCommand.CommandId);
            });
        }
    }

    public async Task HandleConfirmEmailCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var confirmEmailCommand = JsonConvert.DeserializeObject<ConfirmEmailCommand>(dataString);
        if (confirmEmailCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(confirmEmailCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                await _userService.ConfirmEmail(confirmEmailCommand.UserId, confirmEmailCommand.Token);
                await _processedEventService.MarkEventAsProcessed(confirmEmailCommand.CommandId);
            });
        }
    }

    public async Task HandleDeleteUserCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var deleteUserCommand = JsonConvert.DeserializeObject<DeleteUserCommand>(dataString);
        if (deleteUserCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(deleteUserCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                await _userService.DeleteUser(deleteUserCommand.UserId);
                await _processedEventService.MarkEventAsProcessed(deleteUserCommand.CommandId);
            });
        }
    }

    public async Task HandleUpdateUsernameCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updateUsernameCommand = JsonConvert.DeserializeObject<UpdateUsernameCommand>(dataString);
        if (updateUsernameCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updateUsernameCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                await _userService.UpdateUsername(updateUsernameCommand.UserId, updateUsernameCommand.NewUsername);
                await _processedEventService.MarkEventAsProcessed(updateUsernameCommand.CommandId);
            });
        }
    }

    public async Task HandleUpdatePasswordCommand(BasicDeliverEventArgs ea)
    {
        var dataString = Encoding.UTF8.GetString(ea.Body.ToArray());
        var updatePasswordCommand = JsonConvert.DeserializeObject<UpdatePasswordCommand>(dataString);
        if (updatePasswordCommand != null)
        {
            if (await _processedEventService.IsEventProcessed(updatePasswordCommand.CommandId))
            {
                return;
            }

            await _commandHandler.HandleCommandAsync(ea, async () =>
            {
                await _userService.UpdatePassword(updatePasswordCommand.UserId, updatePasswordCommand.NewPassword);
                await _processedEventService.MarkEventAsProcessed(updatePasswordCommand.CommandId);
            });
        }
    }
}
