using MediatR;

namespace FeaneMVC.Application.Commands.Notifications;

public record AddNotificationFilterCommand(string Name) : IRequest<int>;
