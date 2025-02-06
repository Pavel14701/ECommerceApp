using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[Table("users")]
[Index(nameof(Id))]
[Index(nameof(Username))]
[Index(nameof(Email))]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("username", TypeName = "varchar(255)")]
    public required string Username { get; set; }

    [Required]
    [Column("email", TypeName = "varchar(255)")]
    public required string Email { get; set; }

    [Required]
    [Column("password_hash", TypeName = "varchar(512)")]
    public required string PasswordHash { get; set; }

    [Required]
    [Column("salt", TypeName = "varchar(30)")]
    public required string Salt { get; set; }

    [Required]
    [Column("is_admin", TypeName = "boolean")]
    public required bool IsAdmin { get; set; }
}
