using System.ComponentModel.DataAnnotations;
using UserService.Domain.Enums;

namespace UserService.Application.DTOs;

public class UserProfileUpdateRequest
{
    [Required]
    public Guid AuthUserId { get; set; }

    [Required]
    [StringLength(128)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(32)]
    public string? PhoneNumber { get; set; }

    [StringLength(256)]
    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public Role Role { get; set; } = Role.User;
}
