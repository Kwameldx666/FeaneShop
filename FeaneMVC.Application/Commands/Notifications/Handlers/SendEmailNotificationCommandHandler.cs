using FeaneMVC.Application.Common.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using INotification = FeaneMVC.Application.Common.Interfaces.Services.INotification;

namespace FeaneMVC.Application.Commands.Notifications.Handlers;

public class SendEmailNotificationCommandHandler : IRequestHandler<SendEmailNotificationCommand, Unit>
{
    private readonly INotification _notificationService;

    public SendEmailNotificationCommandHandler(INotification notificationService)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public Task<Unit> Handle(SendEmailNotificationCommand request, CancellationToken cancellationToken)
    {
        _notificationService.SendNotification(request.Message, request.RecipientEmail, request.Subject);
        return Unit.Task;
    }
}
