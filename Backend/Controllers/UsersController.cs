using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;

    public UsersController(IAuthService authService)
    {
        _authService = authService;
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
