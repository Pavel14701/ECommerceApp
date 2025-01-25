using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateCommand command)
    {
        var result = await _authService.Authenticate(command.Username, command.Password);
        if (result.Success)
        {
            return Ok(result.Tokens);
        }

        return Unauthorized(result.Message);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _authService.RefreshToken(command.RefreshToken);
        if (result.Success)
        {
            return Ok(result.Tokens);
        }

        return Unauthorized(result.Message);
    }
}
