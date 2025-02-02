using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class authController : ControllerBase
{
    private readonly IMessageSender _messageSender;

    public authController(IMessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateCommand command)
    {
        var commandId = Guid.NewGuid();
        var authenticateCommand = new AuthenticateCommand
        {
            CommandId = commandId,
            Username = command.Username,
            Password = command.Password
        };

        var result = await _messageSender.SendCommandAndGetResponse<AuthResultDto>("auth.exchange", "auth.authenticate", authenticateCommand);
        if (result.Success)
        {
            return Ok(result.Tokens);
        }

        return Unauthorized(result.Message);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var commandId = Guid.NewGuid();
        var refreshTokenCommand = new RefreshTokenCommand
        {
            CommandId = commandId,
            RefreshToken = command.RefreshToken
        };

        var result = await _messageSender.SendCommandAndGetResponse<AuthResultDto>("auth.exchange", "auth.refreshToken", refreshTokenCommand);
        if (result.Success)
        {
            return Ok(result.Tokens);
        }

        return Unauthorized(result.Message);
    }
}
