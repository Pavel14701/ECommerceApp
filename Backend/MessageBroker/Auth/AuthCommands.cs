public class AuthenticateCommand
{
    public Guid CommandId { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class RefreshTokenCommand
{
    public Guid CommandId { get; set; }
    public required string RefreshToken { get; set; }
}
