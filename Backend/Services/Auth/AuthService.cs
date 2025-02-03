using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly IConfiguration _configuration;
    private readonly byte[] _key;

    public AuthService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        IConfiguration configuration
    )
    {
        _dbContextFactory = dbContextFactory;
        _configuration = configuration;
        _key = Encoding.ASCII.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException(
                "JWT key is not configured."
            )
        );
    }

    public async Task<AuthResultDto> Authenticate(AuthDtoParams _user)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = @"
            SELECT * FROM Users 
            WHERE Username = @Username
        ";
        var user = await context.Users
            .FromSqlRaw(
                commandText, 
                new SqlParameter("@Username", _user.Username)
            )
            .SingleOrDefaultAsync();
        if (user == null)
        {
            return new AuthResultDto
            {
                Success = false,
                Message = "Invalid username or password."
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
            return new AuthResultDto
            {
                Success = false,
                Message = "Invalid username or password."
            };
        }
        var userParams = new UserDtoParams
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email
        };
        var tokens = GenerateTokens(userParams);
        var authResult = new AuthResultDto
        {
            Success = true,
            User = user,
            Tokens = tokens,
            Message = "User authenticated successfully."
        };
        return authResult;
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
            return result;
        }
    }

    public TokenResultDto GenerateTokens(UserDtoParams user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user);
        var tokenResult = new TokenResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
        return tokenResult;
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
                return new AuthResultDto { 
                    Success = false, 
                    Message = "Invalid refresh token." 
                };
            }
            using var context = _dbContextFactory.CreateDbContext();
            var commandText = @"
                SELECT * FROM Users 
                WHERE Id = @UserId
            ";
            var user = await context.Users
                .FromSqlRaw(
                    commandText, 
                    new SqlParameter(
                        "@UserId", Guid.Parse(userId)
                    )
                )
                .SingleOrDefaultAsync();
            if (user == null)
            {
                return new AuthResultDto { 
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
            return refreshResult;
        }
        catch (Exception)
        {
            return new AuthResultDto { 
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
            _configuration.GetValue<int>(
                "Jwt:AccessTokenLifetimeMinutes"
            )
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
        var tokenString = tokenHandler.WriteToken(token);
        return tokenString;
    }

    private string GenerateRefreshToken(UserDtoParams user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var notBefore = now.AddSeconds(-1);
        var expires = now.AddDays(_configuration.GetValue<int>(
            "Jwt:RefreshTokenLifetimeDays"
        ));
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
        var tokenString = tokenHandler.WriteToken(token);
        return tokenString;
    }
}
