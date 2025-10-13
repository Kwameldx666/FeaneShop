using MediatR;

namespace FeaneMVC.Application.Commands.Notifications;

public record SendEmailNotificationCommand(string Message, string RecipientEmail, string? Subject) : IRequest<Unit>;
