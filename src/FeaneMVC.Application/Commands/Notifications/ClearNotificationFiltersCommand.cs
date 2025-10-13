using MediatR;

namespace FeaneMVC.Application.Commands.Notifications;

public record ClearNotificationFiltersCommand() : IRequest<Unit>;
