using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StackExchange.Redis;

public class UserService : IUserService
{
    private readonly SessionIterator _sessionIterator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly TimeSpan _tokenLifetime;

    public UserService(SessionIterator sessionIterator, IHttpContextAccessor httpContextAccessor, IConnectionMultiplexer redis, IConfiguration configuration, IEmailService emailService)
    {
        _sessionIterator = sessionIterator;
        _httpContextAccessor = httpContextAccessor;
        _redis = redis;
        _configuration = configuration;
        _emailService = emailService;
        _tokenLifetime = TimeSpan.FromHours(configuration.GetValue<int>("EmailTokenLifetimeHours"));
    }

    public async Task<UserResultDto> GetCurrentUser()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return new UserResultDto { Success = false, Message = "User is not authenticated." };
        }
        var commandText = "SELECT * FROM Users WHERE Id = @UserId";
        var user = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Users
                .FromSqlRaw(commandText, new NpgsqlParameter("@UserId", userId))
                .SingleOrDefaultAsync();
        });
        if (user == null)
        {
            return new UserResultDto { Success = false, Message = "User not found." };
        }
        return new UserResultDto { Success = true, User = user };
    }

    public async Task<RegisterUserResultDto> RegisterUser(string username, string email, string password)
    {
        var salt = GenerateSalt();
        var passwordHash = HashPassword(password, salt);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Salt = salt,
            IsAdmin = false
        };
        await _sessionIterator.ExecuteAsync(async context =>
        {
            var commandText = @"
                INSERT INTO Users (Id, Username, Email, PasswordHash, Salt, IsAdmin)
                VALUES (@Id, @Username, @Email, @PasswordHash, @Salt, @IsAdmin)";
            await context.Database.ExecuteSqlRawAsync(commandText,
                new NpgsqlParameter("@Id", user.Id),
                new NpgsqlParameter("@Username", user.Username),
                new NpgsqlParameter("@Email", user.Email),
                new NpgsqlParameter("@PasswordHash", user.PasswordHash),
                new NpgsqlParameter("@Salt", user.Salt),
                new NpgsqlParameter("@IsAdmin", user.IsAdmin));
        });
        var token = GenerateEmailConfirmationToken(user);
        var db = _redis.GetDatabase();
        await db.StringSetAsync(token, user.Id.ToString(), _tokenLifetime);
        await _emailService.SendEmailAsync(
            email,
            "Email Confirmation",
            $@"Please confirm your email by clicking the link: {
                _configuration["AppSettings:AppBaseUrl"]
            }/confirm-email?token={token}"
        );
        return new RegisterUserResultDto { Success = true, Message = "User registered successfully. Please check your email to confirm your account." };
    }

    public async Task<ConfirmEmailResultDto> ConfirmEmail(Guid userId, string token)
    {
        var db = _redis.GetDatabase();
        var storedUserId = await db.StringGetAsync(token);
        if (storedUserId.IsNullOrEmpty || storedUserId.ToString() != userId.ToString())
        {
            return new ConfirmEmailResultDto { Success = false, Message = "Invalid token or user ID." };
        }
        var commandText = "SELECT * FROM Users WHERE Id = @UserId";
        var user = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Users
                .FromSqlRaw(commandText, new NpgsqlParameter("@UserId", userId))
                .SingleOrDefaultAsync();
        });
        if (user == null)
        {
            return new ConfirmEmailResultDto { Success = false, Message = "Invalid user ID." };
        }

        await db.KeyDeleteAsync(token);
        return new ConfirmEmailResultDto { Success = true, Message = "Email confirmed successfully." };
    }

    public async Task<DeleteUserResultDto> DeleteUser(Guid userId)
    {
        var commandText = "SELECT * FROM Users WHERE Id = @UserId";
        var user = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Users
                .FromSqlRaw(commandText, new NpgsqlParameter("@UserId", userId))
                .SingleOrDefaultAsync();
        });
        if (user != null)
        {
            await _sessionIterator.ExecuteAsync(async context =>
            {
                var deleteCommandText = "DELETE FROM Users WHERE Id = @UserId";
                await context.Database.ExecuteSqlRawAsync(
                    deleteCommandText, new NpgsqlParameter("@UserId", userId)
                );
            });
            return new DeleteUserResultDto { Success = true, Message = "User deleted successfully." };
        }
        return new DeleteUserResultDto { Success = false, Message = "User not found." };
    }

    public async Task<UpdateUserResultDto> UpdateUsername(Guid userId, string newUsername)
    {
        var commandText = "SELECT * FROM Users WHERE Id = @UserId";
        var user = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Users
                .FromSqlRaw(commandText, new NpgsqlParameter("@UserId", userId))
                .SingleOrDefaultAsync();
        });
        if (user != null)
        {
            await _sessionIterator.ExecuteAsync(async context =>
            {
                var updateCommandText = @"
                    UPDATE Users 
                    SET Username = @Username
                    WHERE Id = @UserId
                ";
                await context.Database.ExecuteSqlRawAsync(updateCommandText,
                    new NpgsqlParameter("@Username", newUsername),
                    new NpgsqlParameter("@UserId", userId));
            });
            return new UpdateUserResultDto { Success = true, Message = "Username updated successfully." };
        }
        return new UpdateUserResultDto { Success = false, Message = "User not found." };
    }

    public async Task<UpdateUserResultDto> UpdatePassword(Guid userId, string newPassword)
    {
        var commandText = "SELECT * FROM Users WHERE Id = @UserId";
        var user = await _sessionIterator.QueryAsync(async context =>
        {
            return await context.Users
                .FromSqlRaw(commandText, new NpgsqlParameter("@UserId", userId))
                .SingleOrDefaultAsync();
        });
        if (user != null)
        {
            var salt = GenerateSalt();
            var passwordHash = HashPassword(newPassword, salt);
            await _sessionIterator.ExecuteAsync(async context =>
            {
                var updateCommandText = @"
                    UPDATE Users 
                    SET PasswordHash = @PasswordHash, Salt = @Salt
                    WHERE Id = @UserId
                ";
                await context.Database.ExecuteSqlRawAsync(updateCommandText,
                    new NpgsqlParameter("@PasswordHash", passwordHash),
                    new NpgsqlParameter("@Salt", salt),
                    new NpgsqlParameter("@UserId", userId));
            });
            return new UpdateUserResultDto { Success = true, Message = "Password updated successfully." };
        }
        return new UpdateUserResultDto { Success = false, Message = "User not found." };
    }

    private string GenerateSalt()
    {
        var saltBytes = new byte[22];
        RandomNumberGenerator.Fill(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    private string HashPassword(string password, string salt)
    {
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = Encoding.UTF8.GetBytes(password).Concat(Convert.FromBase64String(salt)).ToArray();
            var hashBytes = sha256.ComputeHash(saltedPassword);
            return Convert.ToBase64String(hashBytes);
        }
    }

    private string GenerateEmailConfirmationToken(User user)
    {
        var secretKey = _configuration["Jwt:Key"]!;
        var combinedString = $"{user.Username}{secretKey}";
        using (var sha256 = SHA256.Create())
        {
            var tokenBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
            return Convert.ToBase64String(tokenBytes);
        }
    }
}
