using System.Threading.Tasks;

public interface IAuthService
{
    Task<AuthResultDto> Authenticate(AuthDtoParams _user);
    Task<AuthResultDto> RefreshToken(RefreshDtoParams _token);
}

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
}
