using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Users;

public record UpdateUserAddressCommand(UserData User, DeliveryAddress Address) : IRequest<OperationResult<UserProfile>>;
