using FeaneMVC.Domain.Entities;
using MediatR;

namespace FeaneMVC.Application.Queries.Notifications;

public record GetNotificationsQuery() : IRequest<IReadOnlyList<Notification>>;
