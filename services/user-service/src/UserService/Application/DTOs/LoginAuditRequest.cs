namespace UserService.Application.DTOs;

public record LoginAuditRequest(Guid UserId, string CookieValue, DateTime LoginTime);