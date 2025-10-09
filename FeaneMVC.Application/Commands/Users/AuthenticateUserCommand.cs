using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Users;

public record AuthenticateUserCommand(string Credential, string Password) : IRequest<OperationResult<UserProfile>>;
