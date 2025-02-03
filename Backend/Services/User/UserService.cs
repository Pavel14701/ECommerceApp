using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using StackExchange.Redis;

public class UserService : IUserService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly TimeSpan _tokenLifetime;

    public UserService(IDbContextFactory<ApplicationDbContext> dbContextFactory, IHttpContextAccessor httpContextAccessor, IConnectionMultiplexer redis, IConfiguration configuration, IEmailService emailService)
    {
        _dbContextFactory = dbContextFactory;
        _httpContextAccessor = httpContextAccessor;
        _redis = redis;
        _configuration = configuration;
        _emailService = emailService;
        _tokenLifetime = TimeSpan.FromHours(configuration.GetValue<int>("EmailTokenLifetimeHours"));
    }

    public async Task<UserResultDto> GetCurrentUser()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(
            ClaimTypes.NameIdentifier
        );
        if (userId == null)
        {
            return new UserResultDto {
                Success = false, Message = "User is not authenticated."
            };
        }
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = "SELECT * FROM Users WHERE Id = @UserId";
        var user = await context.Users
            .FromSqlRaw(commandText, new SqlParameter("@UserId", userId))
            .SingleOrDefaultAsync();

        if (user == null)
        {
            return new UserResultDto { Success = false, Message = "User not found." };
        }

        return new UserResultDto { Success = true, User = user };
    }

    public async Task<RegisterUserResultDto> RegisterUser(
        string username, string email, string password
    )
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
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = @"
            INSERT INTO Users (Id, Username, Email, PasswordHash, Salt, IsAdmin)
            VALUES (@Id, @Username, @Email, @PasswordHash, @Salt, @IsAdmin)";
        await context.Database.ExecuteSqlRawAsync(commandText,
            new SqlParameter("@Id", user.Id),
            new SqlParameter("@Username", user.Username),
            new SqlParameter("@Email", user.Email),
            new SqlParameter("@PasswordHash", user.PasswordHash),
            new SqlParameter("@Salt", user.Salt),
            new SqlParameter("@IsAdmin", user.IsAdmin));
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
            return new ConfirmEmailResultDto {
                Success = false, Message = "Invalid token or user ID."
            };
        }
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = "SELECT * FROM Users WHERE Id = @UserId";
        var user = await context.Users
            .FromSqlRaw(commandText, new SqlParameter("@UserId", userId))
            .SingleOrDefaultAsync();
        if (user == null)
        {
            return new ConfirmEmailResultDto {
                Success = false, Message = "Invalid user ID."
            };
        }
        await db.KeyDeleteAsync(token);
        return new ConfirmEmailResultDto { Success = true, Message = "Email confirmed successfully." };
    }

    public async Task<DeleteUserResultDto> DeleteUser(Guid userId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = "SELECT * FROM Users WHERE Id = @UserId";
        var user = await context.Users
            .FromSqlRaw(commandText, new SqlParameter("@UserId", userId))
            .SingleOrDefaultAsync();
        if (user != null)
        {
            var deleteCommandText = "DELETE FROM Users WHERE Id = @UserId";
            await context.Database.ExecuteSqlRawAsync(
                deleteCommandText, new SqlParameter("@UserId", userId)
            );
            return new DeleteUserResultDto {
                Success = true, Message = "User deleted successfully."
            };
        }
        return new DeleteUserResultDto { Success = false, Message = "User not found." };
    }

    public async Task<UpdateUserResultDto> UpdateUsername(Guid userId, string newUsername)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = "SELECT * FROM Users WHERE Id = @UserId";
        var user = await context.Users
            .FromSqlRaw(commandText, new SqlParameter("@UserId", userId))
            .SingleOrDefaultAsync();
        if (user != null)
        {
            var updateCommandText = @"
                UPDATE Users 
                SET Username = @Username
                WHERE Id = @UserId
            ";
            await context.Database.ExecuteSqlRawAsync(updateCommandText,
                new SqlParameter("@Username", newUsername),
                new SqlParameter("@UserId", userId));
            return new UpdateUserResultDto {
                Success = true, Message = "Username updated successfully."
            };
        }
        return new UpdateUserResultDto { Success = false, Message = "User not found." };
    }

    public async Task<UpdateUserResultDto> UpdatePassword(Guid userId, string newPassword)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var commandText = "SELECT * FROM Users WHERE Id = @UserId";
        var user = await context.Users
            .FromSqlRaw(commandText, new SqlParameter("@UserId", userId))
            .SingleOrDefaultAsync();
        if (user != null)
        {
            var salt = GenerateSalt();
            var passwordHash = HashPassword(newPassword, salt);
            var updateCommandText = @"
                UPDATE Users 
                SET PasswordHash = @PasswordHash, Salt = @Salt
                WHERE Id = @UserId
            ";
            await context.Database.ExecuteSqlRawAsync(updateCommandText,
                new SqlParameter("@PasswordHash", passwordHash),
                new SqlParameter("@Salt", salt),
                new SqlParameter("@UserId", userId));
            return new UpdateUserResultDto {
                Success = true, Message = "Password updated successfully."
            };
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
            var saltedPassword = Encoding.UTF8.GetBytes(password).
                Concat(Convert.FromBase64String(salt)).ToArray();
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
