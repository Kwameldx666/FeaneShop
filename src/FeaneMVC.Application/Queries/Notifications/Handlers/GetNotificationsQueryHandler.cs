using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.Interfaces;
using MediatR;

namespace FeaneMVC.Application.Queries.Notifications.Handlers;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, IReadOnlyList<Notification>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
    }

    public async Task<IReadOnlyList<Notification>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetAllNotificationsAsync();
        return notifications;
    }
}
