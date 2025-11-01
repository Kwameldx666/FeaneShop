namespace UserService.Application.DTOs;

public record AuthenticateRequest(string Credential, string Password);