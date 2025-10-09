using FeaneMVC.Application.Common.Interfaces.Services;
using MediatR;

namespace FeaneMVC.Application.Commands.Sessions.Handlers;

public class SetSessionValueCommandHandler : IRequestHandler<SetSessionValueCommand, Unit>
{
    private readonly ISessionService _sessionService;

    public SetSessionValueCommandHandler(ISessionService sessionService)
    {
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
    }

    public Task<Unit> Handle(SetSessionValueCommand request, CancellationToken cancellationToken)
    {
        _sessionService.SetSession(request.Key, request.Value);
        return Unit.Task;
    }
}
