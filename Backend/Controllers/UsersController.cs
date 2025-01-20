using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public UsersController(IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string username, string email, string password)
    {
        await _userService.RegisterUser(username, email, password);
        return Ok("Registration successful. Please check your email to confirm your account.");
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
    {
        await _userService.ConfirmEmail(userId, token);
        return Ok("Email confirmed successfully.");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _userService.DeleteUser(id);
        return NoContent();
    }

    [HttpPut("{id}/username")]
    [Authorize]
    public async Task<IActionResult> UpdateUsername(Guid id, string newUsername)
    {
        await _userService.UpdateUsername(id, newUsername);
        return NoContent();
    }

    [HttpPut("{id}/password")]
    [Authorize]
    public async Task<IActionResult> UpdatePassword(Guid id, string newPassword)
    {
        await _userService.UpdatePassword(id, newPassword);
        return NoContent();
    }

    [HttpPost("authenticate")]
    public async Task<ActionResult<User>> Authenticate(string username, string password)
    {
        var user = await _authService.Authenticate(username, password);
        if (user == null)
        {
            return Unauthorized();
        }
        return Ok(user);
    }
}
