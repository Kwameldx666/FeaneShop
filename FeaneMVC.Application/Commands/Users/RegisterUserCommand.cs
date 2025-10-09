using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Users;

public record RegisterUserCommand(UserData User) : IRequest<OperationResult<UserProfile>>;
