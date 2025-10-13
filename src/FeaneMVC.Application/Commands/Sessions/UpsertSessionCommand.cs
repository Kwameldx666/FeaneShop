using MediatR;

namespace FeaneMVC.Application.Commands.Sessions;

public record UpsertSessionCommand(string Credential, string CookieValue, DateTimeOffset ExpireTime, bool IsEmail) : IRequest;
