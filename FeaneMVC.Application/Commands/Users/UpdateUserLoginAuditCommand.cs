using MediatR;

namespace FeaneMVC.Application.Commands.Users;

public record UpdateUserLoginAuditCommand(Guid UserId, string CookieValue, DateTime LoginTime) : IRequest<bool>;
