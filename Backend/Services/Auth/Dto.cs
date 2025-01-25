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
