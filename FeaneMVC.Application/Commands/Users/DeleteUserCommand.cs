using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Users;

public record DeleteUserCommand(Guid UserId) : IRequest<OperationResult<UserProfile>>;
