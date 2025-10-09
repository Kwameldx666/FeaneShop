using MediatR;

namespace FeaneMVC.Application.Commands.Sessions;

public record SetUserCookieCommand(Guid UserId, string Credential, bool RememberMe) : IRequest<string>;
