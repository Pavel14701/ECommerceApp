using System.Threading.Tasks;

public interface IAuthService
{
    Task<AuthResultDto> Authenticate(string username, string password);
    Task<AuthResultDto> RefreshToken(string refreshToken);
}

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
}
