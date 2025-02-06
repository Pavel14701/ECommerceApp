using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;



public class AuthDtoParams
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class RefreshDtoParams
{
    public required string RefreshToken { get; set; }
}

public class VerifPasswordDtoParams
{
    public required string Password { get; set; }
    public required string StoredHash { get; set; }
    public required string StoredSalt { get; set; }
}

public class UserDtoParams
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
}




public class AuthResultDto
{
    public bool Success { get; set; }
    public required string Message { get; set; } = string.Empty;
    public User? User { get; set; }
    public TokenResultDto? Tokens { get; set; }
}

public class TokenResultDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}





public interface IAuthService
{
    Task<AuthResultDto> Authenticate(AuthDtoParams _user);
    Task<AuthResultDto> RefreshToken(RefreshDtoParams _token);
}






public class AuthService : IAuthService
{
    private readonly ReadCrud _readCrud;
    private readonly IConfiguration _configuration;
    private readonly byte[] _key;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ReadCrud readCrud,
        IConfiguration configuration,
        ILogger<AuthService> logger
    )
    {
        _readCrud = readCrud;
        _configuration = configuration;
        _key = Encoding.ASCII.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException(
                "JWT key is not configured."
            )
        );
        _logger = logger;
    }

    public async Task<AuthResultDto> Authenticate(AuthDtoParams _user)
    {
        try
        {
            var user = await _readCrud.FindUserByUsername(_user.Username);
            if (user == null)
            {
                _logger.LogWarning("Invalid username or password for user: {Username}", _user.Username);
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Invalid username"
                };
            }
            var verifParams = new VerifPasswordDtoParams
            {
                Password = _user.Password,
                StoredHash = user.PasswordHash,
                StoredSalt = user.Salt
            };
            if (!VerifyPasswordHash(verifParams))
            {
                _logger.LogWarning("Invalid password for user: {Username}", _user.Username);
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Invalid password."
                };
            }
            var userParams = new UserDtoParams
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email
            };
            var tokens = GenerateTokens(userParams);
            _logger.LogInformation("User authenticated successfully: {Username}", user.Username);
            return new AuthResultDto
            {
                Success = true,
                User = user,
                Tokens = tokens,
                Message = "User authenticated successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during authentication for user: {Username}", _user.Username);
            return new AuthResultDto
            {
                Success = false,
                Message = "An error occurred during authentication."
            };
        }
    }

    private bool VerifyPasswordHash(VerifPasswordDtoParams _params)
    {
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = Encoding.UTF8.GetBytes(_params.Password).Concat(
                Convert.FromBase64String(_params.StoredSalt)
            ).ToArray();
            var saltedHash = sha256.ComputeHash(saltedPassword);
            var inputPasswordHash = Convert.ToBase64String(saltedHash);
            var result = inputPasswordHash == _params.StoredHash;
            _logger.LogInformation("Password verification result: {Result}", result);
            return result;
        }
    }

    public TokenResultDto GenerateTokens(UserDtoParams user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user);
        _logger.LogInformation("Tokens generated for user: {Username}", user.Username);
        return new TokenResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResultDto> RefreshToken(RefreshDtoParams _token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(
                _token.RefreshToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(_key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false
                }, out SecurityToken validatedToken
            );
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Invalid refresh token.");
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Invalid refresh token."
                };
            }
            var user = await _readCrud.FindUserById(Guid.Parse(userId));
            if (user == null)
            {
                _logger.LogWarning("User not found for refresh token: {UserId}", userId);
                return new AuthResultDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }
            var userParams = new UserDtoParams
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email
            };
            var newAccessToken = GenerateAccessToken(userParams);
            var newRefreshToken = GenerateRefreshToken(userParams);
            _logger.LogInformation("Token refreshed successfully for user: {Username}", user.Username);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during token refresh.");
            return new AuthResultDto
            {
                Success = false,
                Message = "Invalid refresh token."
            };
        }
    }

    private string GenerateAccessToken(UserDtoParams user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var notBefore = now.AddSeconds(-1);
        var expires = now.AddMinutes(
            _configuration.GetValue<int>("Jwt:AccessTokenLifetimeMinutes")
        );
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
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature
            )
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        _logger.LogInformation("Access token generated for user: {Username}", user.Username);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken(UserDtoParams user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var notBefore = now.AddSeconds(-1);
        var expires = now.AddDays(
            _configuration.GetValue<int>("Jwt:RefreshTokenLifetimeDays")
        );
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
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        _logger.LogInformation("Refresh token generated for user: {Username}", user.Username);
        return tokenHandler.WriteToken(token);
    }
}
