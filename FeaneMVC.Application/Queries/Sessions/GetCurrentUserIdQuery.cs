using MediatR;

namespace FeaneMVC.Application.Queries.Sessions;

public record GetCurrentUserIdQuery() : IRequest<Guid>;
