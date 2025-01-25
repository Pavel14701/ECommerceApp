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
        if (user == null || !VerifyPasswordHash(password, user.PasswordHash))
        {
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
                ValidateLifetime = false // we are only verifying the token signature, not its expiration
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

    private string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
            }),
            Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:AccessTokenLifetimeMinutes")),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
            }),
            Expires = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenLifetimeDays")),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
