using System.ComponentModel.DataAnnotations;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

public class UserData
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    public string? NormalizedUserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public Role Roles { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public bool LockoutEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public string? Credential { get; set; }

    public bool IsActive { get; set; }

    public DateTime FirstRegisterTime { get; set; }

    public DateTime? FirstLoginTime { get; set; }

    public string? IP { get; set; }

    public string? CookieValue { get; set; }

    public string? Address { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }
}
