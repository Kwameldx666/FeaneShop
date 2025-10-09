using FeaneMVC.Domain.Interfaces;
using MediatR;

namespace FeaneMVC.Application.Commands.Notifications.Handlers;

public class ClearNotificationFiltersCommandHandler : IRequestHandler<ClearNotificationFiltersCommand, Unit>
{
    private readonly INotificationRepository _notificationRepository;

    public ClearNotificationFiltersCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
    }

    public async Task<Unit> Handle(ClearNotificationFiltersCommand request, CancellationToken cancellationToken)
    {
        await _notificationRepository.ClearFiltersAsync();
        return Unit.Value;
    }
}
