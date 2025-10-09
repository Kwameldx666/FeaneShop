using FeaneMVC.Domain.Interfaces;
using FeaneMVC.Domain.Entities;
using MediatR;

namespace FeaneMVC.Application.Queries.Notifications.Handlers;

public class GetNotificationFiltersQueryHandler : IRequestHandler<GetNotificationFiltersQuery, IReadOnlyList<Filter>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationFiltersQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
    }

    public async Task<IReadOnlyList<Filter>> Handle(GetNotificationFiltersQuery request, CancellationToken cancellationToken)
    {
        var filters = await _notificationRepository.GetAllFiltersAsync();
        return filters;
    }
}
