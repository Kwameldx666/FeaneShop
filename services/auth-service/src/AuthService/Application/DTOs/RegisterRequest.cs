using System.ComponentModel.DataAnnotations;
using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs;

public class RegisterRequest
{
    [Required]
    [MaxLength(256)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public Role Role { get; set; } = Role.User;
}
