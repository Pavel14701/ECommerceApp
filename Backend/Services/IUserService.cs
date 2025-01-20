using System.Threading.Tasks;

public interface IUserService
{
    Task<User> GetCurrentUser();
}
