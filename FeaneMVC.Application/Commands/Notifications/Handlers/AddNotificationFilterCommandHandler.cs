using FeaneMVC.Domain.Interfaces;
using MediatR;

namespace FeaneMVC.Application.Commands.Notifications.Handlers;

public class AddNotificationFilterCommandHandler : IRequestHandler<AddNotificationFilterCommand, int>
{
    private readonly INotificationRepository _notificationRepository;

    public AddNotificationFilterCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
    }

    public Task<int> Handle(AddNotificationFilterCommand request, CancellationToken cancellationToken)
    {
        return _notificationRepository.AddFilterAsync(request.Name);
    }
}
