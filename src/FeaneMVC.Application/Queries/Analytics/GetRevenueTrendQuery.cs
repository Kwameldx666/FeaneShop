using FeaneMVC.Application.DTOs.Analytics;
using MediatR;

namespace FeaneMVC.Application.Queries.Analytics;

public record GetRevenueTrendQuery(DateTime StartDate, DateTime EndDate) : IRequest<IReadOnlyList<RevenueTrendPoint>>;
