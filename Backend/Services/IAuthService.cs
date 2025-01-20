using System.Threading.Tasks;

public interface IAuthService
{
    Task<User?> Authenticate(string username, string password);
}
