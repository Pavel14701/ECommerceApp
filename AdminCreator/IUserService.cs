using System;
using System.Threading.Tasks;

public interface IAdminService
{
    Task<RegisterAdminResultDto> RegisterUser(string username, string email, string password, bool isAdmin = false);
}
