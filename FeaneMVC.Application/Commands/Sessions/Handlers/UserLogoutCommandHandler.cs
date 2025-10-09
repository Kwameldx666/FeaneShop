using FeaneMVC.Application.Common.Interfaces.Services;
using MediatR;
using System.Threading.Tasks;

namespace FeaneMVC.Application.Commands.Sessions.Handlers;

public class UserLogoutCommandHandler : IRequestHandler<UserLogoutCommand, Unit>
{
    private readonly ISessionService _sessionService;

    public UserLogoutCommandHandler(ISessionService sessionService)
    {
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
    }

    public Task<Unit> Handle(UserLogoutCommand request, CancellationToken cancellationToken)
    {
        _sessionService.UserLogout();
        return Unit.Task;
    }
}
