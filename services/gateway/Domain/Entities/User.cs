using System.ComponentModel.DataAnnotations;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class User
{
    [Key] public Guid Id { get; set; }

    [Required] [MaxLength(256)] public string Username { get; set; } = string.Empty;

    [MaxLength(256)] public string? NormalizedUserName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(256)] public string? NormalizedEmail { get; set; }

    [Required] [MinLength(6)] public string Password { get; set; } = string.Empty;

    [Required] public Role Role { get; set; } = Role.User;

    public bool EmailConfirmed { get; set; }

    [MaxLength(256)] public string? SecurityStamp { get; set; }

    [MaxLength(256)] public string? ConcurrencyStamp { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public bool LockoutEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime FirstRegisterTime { get; set; } = DateTime.UtcNow;

    public DateTime? FirstLoginTime { get; set; }

    [MaxLength(64)] public string? CookieValue { get; set; }
}