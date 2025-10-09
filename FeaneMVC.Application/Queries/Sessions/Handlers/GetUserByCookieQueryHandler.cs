using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Domain.Entities;
using MediatR;

namespace FeaneMVC.Application.Queries.Sessions.Handlers;

public class GetUserByCookieQueryHandler : IRequestHandler<GetUserByCookieQuery, UserData?>
{
    private readonly ISessionRepository _sessionRepository;

    public GetUserByCookieQueryHandler(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
    }

    public Task<UserData?> Handle(GetUserByCookieQuery request, CancellationToken cancellationToken)
    {
        return _sessionRepository.GetUserByCookieAsync(request.CookieValue, cancellationToken);
    }
}
