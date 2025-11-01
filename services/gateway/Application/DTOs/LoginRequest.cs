using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs;

public class LoginRequest
{
    [Required] public string Credential { get; set; } = string.Empty;

    [Required] public string Password { get; set; } = string.Empty;
}