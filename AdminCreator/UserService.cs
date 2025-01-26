using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class AdminService : IAdminService
{
    private readonly ApplicationAdminDbContext _context;

    public AdminService(ApplicationAdminDbContext context)
    {
        _context = context;
    }

    public async Task<RegisterAdminResultDto> RegisterUser(string username, string email, string password, bool isAdmin = false)
    {
        var salt = GenerateSalt();
        var passwordHash = HashPassword(password, salt);

        var user = new ApplicationAdmin
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Salt = salt,
            IsAdmin = isAdmin
        };

        _context.ApplicationAdmin.Add(user);
        await _context.SaveChangesAsync();

        return new RegisterAdminResultDto { Success = true, Message = "User registered successfully." };
    }

    private string GenerateSalt()
    {
        var saltBytes = new byte[18];
        RandomNumberGenerator.Fill(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    private string HashPassword(string password, string salt)
    {
        using (var sha256 = SHA256.Create())
        {
            var combinedPassword = password + salt;
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedPassword));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
