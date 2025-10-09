using FeaneMVC.Application.DTOs.Analytics;

namespace FeaneMVC.Application.Common.Interfaces.Services;

public interface IAnalyticsService
{
    Task<AnalyticsSummary> GetSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);

    Task<IReadOnlyList<RevenueTrendPoint>> GetRevenueTrendAsync(DateTime startDate, DateTime endDate);
}
