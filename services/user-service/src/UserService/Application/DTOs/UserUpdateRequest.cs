using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using UserService.Domain.Enums;

namespace UserService.Application.DTOs;

public class UserUpdateRequest
{
    [Required] [MaxLength(256)] public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Role Role { get; set; } = Role.User;

    public bool IsActive { get; set; } = true;

    [MaxLength(512)] public string? Address { get; set; } = string.Empty;

    [MaxLength(64)] public string? PhoneNumber { get; set; } = string.Empty;
}