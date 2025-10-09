using FeaneMVC.Application.Common.Interfaces.Services;
using MediatR;

namespace FeaneMVC.Application.Commands.Sessions.Handlers;

public class SetUserCookieCommandHandler : IRequestHandler<SetUserCookieCommand, string>
{
    private readonly ISessionService _sessionService;

    public SetUserCookieCommandHandler(ISessionService sessionService)
    {
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
    }

    public Task<string> Handle(SetUserCookieCommand request, CancellationToken cancellationToken)
    {
        return _sessionService.SetUserCookieAsync(request.UserId, request.Credential, request.RememberMe, cancellationToken);
    }
}
