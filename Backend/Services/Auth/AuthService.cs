using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory _dbContextFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly byte[] _key;

    public AuthService(IDbContextFactory dbContextFactory, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _configuration = configuration;
        _logger = logger;
        _key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured."));
    }

    public async Task<AuthResultDto> Authenticate(string username, string password)
    {
        _logger.LogInformation("Start authentication service with args: Username = {Username}, Password = {Password}", username, password);

        try
        {
            using var context = _dbContextFactory.CreateDbContext();
            var user = await context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                _logger.LogWarning("User not found: {Username}", username);
                return new AuthResultDto { Success = false, Message = "Invalid username or password." };
            }

            _logger.LogInformation("Found user: {Username}", user.Username);
            _logger.LogInformation("Salt: {Salt}", user.Salt);
            _logger.LogInformation("Stored hashed password: {PasswordHash}", user.PasswordHash);

            if (!VerifyPasswordHash(password, user.PasswordHash, user.Salt))
            {
                _logger.LogWarning("Password hash verification failed for user: {Username}", username);
                return new AuthResultDto { Success = false, Message = "Invalid username or password." };
            }

            var tokens = GenerateTokens(user);

            var authResult = new AuthResultDto
            {
                Success = true,
                User = user,
                Tokens = tokens,
                Message = "User authenticated successfully."
            };

            _logger.LogInformation("End authentication service with result: {AuthResult}", JsonConvert.SerializeObject(authResult));
            return authResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication service");
            throw;
        }
    }

    private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
    {
        _logger.LogInformation("Start password hash verification");
        try
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = Encoding.UTF8.GetBytes(password).Concat(Convert.FromBase64String(storedSalt)).ToArray();
                var saltedHash = sha256.ComputeHash(saltedPassword);
                var inputPasswordHash = Convert.ToBase64String(saltedHash);

                _logger.LogInformation("Input hash: {InputHash}", inputPasswordHash);
                _logger.LogInformation("Stored hash: {StoredHash}", storedHash);
                var result = inputPasswordHash == storedHash;
                _logger.LogInformation("End password hash verification with result: {Result}", result);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password hash verification");
            throw;
        }
    }

    public TokenResultDto GenerateTokens(User user)
    {
        _logger.LogInformation("Start token generation for user: {Username}", user.Username);

        try
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(user);

            var tokenResult = new TokenResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            _logger.LogInformation("End token generation with result: {TokenResult}", JsonConvert.SerializeObject(tokenResult));
            return tokenResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token generation");
            throw;
        }
    }

    public async Task<AuthResultDto> RefreshToken(string refreshToken)
    {
        _logger.LogInformation("Start refresh token service with token: {RefreshToken}", refreshToken);

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false
            }, out SecurityToken validatedToken);

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Invalid refresh token: {RefreshToken}", refreshToken);
                return new AuthResultDto { Success = false, Message = "Invalid refresh token." };
            }

            using var context = _dbContextFactory.CreateDbContext();
            var user = await context.Users.FindAsync(Guid.Parse(userId));
            if (user == null)
            {
                _logger.LogWarning("User not found for refresh token: {RefreshToken}", refreshToken);
                return new AuthResultDto { Success = false, Message = "User not found." };
            }

            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken(user);

            var refreshResult = new AuthResultDto
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

            _logger.LogInformation("End refresh token service with result: {RefreshResult}", JsonConvert.SerializeObject(refreshResult));
            return refreshResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token: {RefreshToken}", refreshToken);
            return new AuthResultDto { Success = false, Message = "Invalid refresh token." };
        }
    }

    private string GenerateAccessToken(User user)
    {
        _logger.LogInformation("Start generating access token for user: {Username}", user.Username);

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;
            var notBefore = now.AddSeconds(-1);
            var expires = now.AddMinutes(_configuration.GetValue<int>("Jwt:AccessTokenLifetimeMinutes"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                }),
                NotBefore = notBefore,
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("End generating access token with result: {Token}", tokenString);
            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating access token for user: {Username}", user.Username);
            throw;
        }
    }

    private string GenerateRefreshToken(User user)
    {
        _logger.LogInformation("Start generating refresh token for user: {Username}", user.Username);

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;
            var notBefore = now.AddSeconds(-1);
            var expires = now.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenLifetimeDays"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                }),
                NotBefore = notBefore,
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("End generating refresh token with result: {Token}", tokenString);
            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating refresh token for user: {Username}", user.Username);
            throw;
        }
    }
}
