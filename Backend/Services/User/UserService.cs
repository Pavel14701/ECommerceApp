using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly TimeSpan _tokenLifetime;

    public UserService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IConnectionMultiplexer redis, IConfiguration configuration, IEmailService emailService)
    {
        _context = context;
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

        var user = await _context.Users.SingleOrDefaultAsync(u => u.Id.ToString() == userId);
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

        var token = GenerateEmailConfirmationToken(user);
        var db = _redis.GetDatabase();
        await db.StringSetAsync(token, user.Id.ToString(), _tokenLifetime);
        await _emailService.SendEmailAsync(email, "Email Confirmation", $"Please confirm your email by clicking the link: {_configuration["AppSettings:AppBaseUrl"]}/confirm-email?token={token}");
        await _context.SaveChangesAsync();

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

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new ConfirmEmailResultDto { Success = false, Message = "Invalid user ID." };
        }

        await _context.SaveChangesAsync();
        await db.KeyDeleteAsync(token);

        return new ConfirmEmailResultDto { Success = true, Message = "Email confirmed successfully." };
    }

    public async Task<DeleteUserResultDto> DeleteUser(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return new DeleteUserResultDto { Success = true, Message = "User deleted successfully." };
        }

        return new DeleteUserResultDto { Success = false, Message = "User not found." };
    }

    public async Task<UpdateUserResultDto> UpdateUsername(Guid userId, string newUsername)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.Username = newUsername;
            await _context.SaveChangesAsync();
            return new UpdateUserResultDto { Success = true, Message = "Username updated successfully." };
        }

        return new UpdateUserResultDto { Success = false, Message = "User not found." };
    }

    public async Task<UpdateUserResultDto> UpdatePassword(Guid userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            var salt = GenerateSalt();
            var passwordHash = HashPassword(newPassword, salt);
            user.PasswordHash = passwordHash;
            user.Salt = salt;
            await _context.SaveChangesAsync();
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
            var combinedPassword = password + salt;
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedPassword));
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
