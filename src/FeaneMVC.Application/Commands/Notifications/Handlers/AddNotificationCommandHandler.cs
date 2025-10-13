using FeaneMVC.Domain.Interfaces;
using MediatR;

namespace FeaneMVC.Application.Commands.Notifications.Handlers;

public class AddNotificationCommandHandler : IRequestHandler<AddNotificationCommand, int>
{
    private readonly INotificationRepository _notificationRepository;

    public AddNotificationCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
    }

    public Task<int> Handle(AddNotificationCommand request, CancellationToken cancellationToken)
    {
        return _notificationRepository.AddNotificationAsync(request.Content);
    }
}
