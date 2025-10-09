using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Queries.Users;

public record GetUserAddressQuery(Guid UserId)
    : IRequest<OperationResult<DeliveryAddress>>;