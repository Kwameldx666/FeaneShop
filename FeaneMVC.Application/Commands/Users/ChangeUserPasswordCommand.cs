using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Users;

public record ChangeUserPasswordCommand(string Email) : IRequest<OperationResult<UserProfile>>;
