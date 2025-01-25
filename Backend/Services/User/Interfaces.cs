using System.Threading.Tasks;

public interface IUserService
{
    Task<UserResultDto> GetCurrentUser();
    Task<RegisterUserResultDto> RegisterUser(string username, string email, string password);
    Task<ConfirmEmailResultDto> ConfirmEmail(Guid userId, string token);
    Task<DeleteUserResultDto> DeleteUser(Guid userId);
    Task<UpdateUserResultDto> UpdateUsername(Guid userId, string newUsername);
    Task<UpdateUserResultDto> UpdatePassword(Guid userId, string newPassword);
}
