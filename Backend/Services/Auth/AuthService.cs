using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResultDto> Authenticate(string username, string password)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            Console.WriteLine("User not found.");
            return new AuthResultDto { Success = false, Message = "Invalid username or password." };
        }

        Console.WriteLine($"Username: {user.Username}");
        Console.WriteLine($"Salt: {user.Salt}");
        Console.WriteLine($"Stored hashed password: {user.PasswordHash}");

        if (!VerifyPasswordHash(password, user.PasswordHash, user.Salt))
        {
            Console.WriteLine("Password hash verification failed.");
            return new AuthResultDto { Success = false, Message = "Invalid username or password." };
        }

        var tokens = GenerateTokens(user);

        return new AuthResultDto
        {
            Success = true,
            User = user,
            Tokens = tokens,
            Message = "User authenticated successfully."
        };
    }

    private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
    {
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = Encoding.UTF8.GetBytes(password).Concat(Convert.FromBase64String(storedSalt)).ToArray();
            var saltedHash = sha256.ComputeHash(saltedPassword);
            var inputPasswordHash = Convert.ToBase64String(saltedHash);
            Console.WriteLine($"Input hash: {inputPasswordHash}");
            Console.WriteLine($"Stored hash: {storedHash}");
            return inputPasswordHash == storedHash;
        }
    }

    public TokenResultDto GenerateTokens(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user);

        return new TokenResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResultDto> RefreshToken(string refreshToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false
            }, out SecurityToken validatedToken);

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return new AuthResultDto { Success = false, Message = "Invalid refresh token." };
            }

            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user == null)
            {
                return new AuthResultDto { Success = false, Message = "User not found." };
            }

            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken(user);

            return new AuthResultDto
            {
                Success = true,
                User = user,
                Tokens = new TokenResultDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                },
                Message = "Token refreshed successfully."
            };
        }
        catch
        {
            return new AuthResultDto { Success = false, Message = "Invalid refresh token." };
        }
    }

    private string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

        // Установите значения NotBefore и Expires
        var now = DateTime.UtcNow;
        var notBefore = now.AddSeconds(-1); // Смещение на 1 секунду назад
        var expires = now.AddMinutes(_configuration.GetValue<int>("Jwt:AccessTokenLifetimeMinutes"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
            }),
            NotBefore = notBefore, // Установите NotBefore с небольшим смещением
            Expires = expires, // Установите Expires на более позднее время
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }



    private string GenerateRefreshToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

        // Установите значения NotBefore и Expires
        var now = DateTime.UtcNow;
        var notBefore = now.AddSeconds(-1); // Смещение на 1 секунду назад
        var expires = now.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenLifetimeDays"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
            }),
            NotBefore = notBefore, // Установите NotBefore с небольшим смещением
            Expires = expires, // Установите Expires на более позднее время
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
