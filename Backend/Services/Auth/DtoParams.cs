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