using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> Authenticate(string username, string password) // Изменено на Task<User?>
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
        if (user == null || !VerifyPasswordHash(password, user.PasswordHash))
        {
            return null;
        }

        return user;
    }

    private bool VerifyPasswordHash(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        var salt = Convert.FromBase64String(parts[0]);
        var storedPasswordHash = parts[1];

        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = Encoding.UTF8.GetBytes(password);
            var saltedHash = sha256.ComputeHash(salt.Concat(saltedPassword).ToArray());
            var inputPasswordHash = Convert.ToBase64String(saltedHash);
            return inputPasswordHash == storedPasswordHash;
        }
    }
}
