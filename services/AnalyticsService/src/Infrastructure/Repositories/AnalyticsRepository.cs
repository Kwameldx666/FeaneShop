using AnalyticsService.Application.DTOs;
using AnalyticsService.Application.Interfaces;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsService.Infrastructure.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<AnalyticsRepository> _logger;

    public AnalyticsRepository(AnalyticsDbContext context, ILogger<AnalyticsRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DashboardResponse> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var orderStats = await _context.OrderStatistics
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .ToListAsync();

        var productStats = await _context.ProductStatistics
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .GroupBy(s => new { s.ProductId, s.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                TotalSold = g.Sum(s => s.TotalQuantitySold),
                Revenue = g.Sum(s => s.TotalRevenue)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(5)
            .ToListAsync();

        var dailyRevenue = orderStats
            .GroupBy(s => s.Date.Date)
            .Select(g => new DailyRevenueDto
            {
                Date = g.Key,
                Revenue = g.Sum(s => s.TotalRevenue),
                Orders = g.Sum(s => s.TotalOrders)
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new DashboardResponse
        {
            TotalRevenue = orderStats.Sum(s => s.TotalRevenue),
            TotalOrders = orderStats.Sum(s => s.TotalOrders),
            CompletedOrders = orderStats.Sum(s => s.CompletedOrders),
            CancelledOrders = orderStats.Sum(s => s.CancelledOrders),
            AverageOrderValue = orderStats.Any()
                ? orderStats.Average(s => s.AverageOrderValue)
                : 0,
            TopProducts = productStats,
            RevenueChart = dailyRevenue
        };
    }

    public async Task<RevenueReportResponse> GetRevenueReportAsync(DateTime startDate, DateTime endDate)
    {
        var stats = await _context.OrderStatistics
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .ToListAsync();

        var dailyBreakdown = stats
            .GroupBy(s => s.Date.Date)
            .Select(g => new DailyRevenueDto
            {
                Date = g.Key,
                Revenue = g.Sum(s => s.TotalRevenue),
                Orders = g.Sum(s => s.TotalOrders)
            })
            .OrderBy(d => d.Date)
            .ToList();

        var totalOrders = stats.Sum(s => s.TotalOrders);

        return new RevenueReportResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = stats.Sum(s => s.TotalRevenue),
            TotalOrders = totalOrders,
            AverageOrderValue = totalOrders > 0
                ? stats.Sum(s => s.TotalRevenue) / totalOrders
                : 0,
            DailyBreakdown = dailyBreakdown
        };
    }

    public async Task<ProductPerformanceResponse> GetProductPerformanceAsync(int topN = 10)
    {
        var products = await _context.ProductStatistics
            .GroupBy(s => new { s.ProductId, s.ProductName })
            .Select(g => new ProductStatisticsDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                TotalOrders = g.Sum(s => s.TotalOrders),
                TotalQuantitySold = g.Sum(s => s.TotalQuantitySold),
                TotalRevenue = g.Sum(s => s.TotalRevenue)
            })
            .OrderByDescending(p => p.TotalRevenue)
            .Take(topN)
            .ToListAsync();

        return new ProductPerformanceResponse
        {
            Products = products
        };
    }

    public async Task RecordEventAsync(string eventType, string entityType, Guid? entityId, string data, Guid? userId)
    {
        var analyticsEvent = new AnalyticsEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId,
            Data = data,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };

        _context.AnalyticsEvents.Add(analyticsEvent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Analytics event recorded: {EventType} for {EntityType} {EntityId}",
            eventType, entityType, entityId);
    }
}