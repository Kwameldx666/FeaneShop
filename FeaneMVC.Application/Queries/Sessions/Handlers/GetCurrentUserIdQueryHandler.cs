using FeaneMVC.Application.Common.Interfaces.Services;
using MediatR;

namespace FeaneMVC.Application.Queries.Sessions.Handlers;

public class GetCurrentUserIdQueryHandler : IRequestHandler<GetCurrentUserIdQuery, Guid>
{
    private readonly ISessionService _sessionService;

    public GetCurrentUserIdQueryHandler(ISessionService sessionService)
    {
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
    }

    public Task<Guid> Handle(GetCurrentUserIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _sessionService.GetUserId();
        return Task.FromResult(userId);
    }
}
