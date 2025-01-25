public class RegisterUserCommand
{
    public Guid CommandId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class ConfirmEmailCommand
{
    public Guid CommandId { get; set; }
    public Guid UserId { get; set; }
    public required string Token { get; set; }
}

public class DeleteUserCommand
{
    public Guid CommandId { get; set; }
    public Guid UserId { get; set; }
}

public class UpdateUsernameCommand
{
    public Guid CommandId { get; set; }
    public Guid UserId { get; set; }
    public required string NewUsername { get; set; }
}

public class UpdatePasswordCommand
{
    public Guid CommandId { get; set; }
    public Guid UserId { get; set; }
    public required string NewPassword { get; set; }
}
