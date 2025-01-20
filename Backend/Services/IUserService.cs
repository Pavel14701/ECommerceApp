using System.Threading.Tasks;

public interface IUserService
{
    Task<User> GetCurrentUser();
    Task RegisterUser(string username, string email, string password);
    Task ConfirmEmail(Guid userId, string token);
    Task DeleteUser(Guid userId);
    Task UpdateUsername(Guid userId, string newUsername);
    Task UpdatePassword(Guid userId, string newPassword);
}
