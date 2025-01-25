public class UserResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; } = string.Empty;
    public User? User { get; set; }
}

public class RegisterUserResultDto
{
    public bool Success { get; set; }
    public required string Message { get; set; }
}

public class ConfirmEmailResultDto
{
    public bool Success { get; set; }
    public required string Message { get; set; }
}

public class DeleteUserResultDto
{
    public bool Success { get; set; }
    public required string Message { get; set; }
}

public class UpdateUserResultDto
{
    public bool Success { get; set; }
    public required string Message { get; set; }
}
