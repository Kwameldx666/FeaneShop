using FeaneMVC.Domain.Entities;
using MediatR;

namespace FeaneMVC.Application.Queries.Notifications;

public record GetNotificationFiltersQuery() : IRequest<IReadOnlyList<Filter>>;
