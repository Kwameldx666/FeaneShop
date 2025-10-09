using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Users;

public record UpdateUserCommand(UserData User) : IRequest<OperationResult<UserProfile>>;
