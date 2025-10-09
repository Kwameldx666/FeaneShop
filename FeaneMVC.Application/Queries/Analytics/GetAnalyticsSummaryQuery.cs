using FeaneMVC.Application.DTOs.Analytics;
using MediatR;

namespace FeaneMVC.Application.Queries.Analytics;

public record GetAnalyticsSummaryQuery(DateTime? StartDate, DateTime? EndDate) : IRequest<AnalyticsSummary>;
