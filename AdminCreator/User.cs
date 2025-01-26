using System;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Users")]
public class ApplicationAdmin
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}
