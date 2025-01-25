using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMessageSender _messageSender;

    public UserController(IMessageSender messageSender)
    {
        _messageSender = messageSender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserCommand command)
    {
        var commandId = Guid.NewGuid();
        command.CommandId = commandId;

        var result = await _messageSender.SendCommandAndGetResponse<RegisterUserResultDto>("users.exchange", "users.register", command);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand command)
    {
        var commandId = Guid.NewGuid();
        command.CommandId = commandId;

        var result = await _messageSender.SendCommandAndGetResponse<ConfirmEmailResultDto>("users.exchange", "users.confirmemail", command);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var commandId = Guid.NewGuid();
        var deleteUserCommand = new DeleteUserCommand
        {
            CommandId = commandId,
            UserId = userId
        };

        var result = await _messageSender.SendCommandAndGetResponse<DeleteUserResultDto>("users.exchange", "users.delete", deleteUserCommand);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpPut("{userId}/username")]
    public async Task<IActionResult> UpdateUsername(Guid userId, [FromBody] UpdateUsernameCommand command)
    {
        if (userId != command.UserId)
        {
            return BadRequest("User ID mismatch.");
        }

        var commandId = Guid.NewGuid();
        command.CommandId = commandId;

        var result = await _messageSender.SendCommandAndGetResponse<UpdateUserResultDto>("users.exchange", "users.updateusername", command);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpPut("{userId}/password")]
    public async Task<IActionResult> UpdatePassword(Guid userId, [FromBody] UpdatePasswordCommand command)
    {
        if (userId != command.UserId)
        {
            return BadRequest("User ID mismatch.");
        }

        var commandId = Guid.NewGuid();
        command.CommandId = commandId;

        var result = await _messageSender.SendCommandAndGetResponse<UpdateUserResultDto>("users.exchange", "users.updatepassword", command);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpGet("current")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _messageSender.SendCommandAndGetResponse<UserResultDto>("users.exchange", "users.current", new {});
        if (result.Success)
        {
            return Ok(result.User);
        }
        else
        {
            return Unauthorized(result.Message);
        }
    }
}
