using AnalyticsService.Application.DTOs;

namespace AnalyticsService.Application.Interfaces;

public interface IAnalyticsRepository
{
    Task<DashboardResponse> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<RevenueReportResponse> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
    Task<ProductPerformanceResponse> GetProductPerformanceAsync(int topN = 10);
    Task RecordEventAsync(string eventType, string entityType, Guid? entityId, string data, Guid? userId);
}