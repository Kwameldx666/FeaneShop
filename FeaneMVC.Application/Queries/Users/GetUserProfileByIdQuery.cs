using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Queries.Users;

public record GetUserProfileByIdQuery(Guid UserId) : IRequest<OperationResult<UserProfile>>;
