using MediatR;

namespace FeaneMVC.Application.Commands.Notifications;

public record AddNotificationCommand(string Content) : IRequest<int>;
