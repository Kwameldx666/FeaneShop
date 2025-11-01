using AnalyticsService.Application.DTOs;
using AnalyticsService.Application.Interfaces;
using AnalyticsService.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AnalyticsService.Tests.Controllers;

public class AnalyticsControllerExtendedTests
{
    private readonly Mock<IAnalyticsRepository> _mockRepository;
    private readonly Mock<ILogger<AnalyticsController>> _mockLogger;
    private readonly AnalyticsController _controller;

    public AnalyticsControllerExtendedTests()
    {
        _mockRepository = new Mock<IAnalyticsRepository>();
        _mockLogger = new Mock<ILogger<AnalyticsController>>();
        _controller = new AnalyticsController(_mockRepository.Object, _mockLogger.Object);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(14)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(90)]
    public async Task GetDashboard_WithDifferentDateRanges_ReturnsData(int days)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var endDate = DateTime.UtcNow;

        var dashboardData = new DashboardResponse
        {
            TotalRevenue = 1000.00m * days,
            TotalOrders = 10 * days,
            AverageOrderValue = 100.00m,
            TopProducts = new List<TopProductDto>()
        };

        _mockRepository.Setup(r => r.GetDashboardDataAsync(startDate, endDate))
            .ReturnsAsync(dashboardData);

        var result = await _controller.GetDashboard(startDate, endDate);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    [InlineData(25)]
    public async Task GetProductPerformance_WithDifferentTopN_ReturnsData(int topN)
    {
        var productPerformance = new ProductPerformanceResponse
        {
            Products = new List<ProductStatisticsDto>()
        };

        for (int i = 0; i < topN; i++)
        {
            productPerformance.Products.Add(new ProductStatisticsDto
            {
                ProductId = Guid.NewGuid(),
                ProductName = $"Product {i}",
                TotalOrders = 100 - i,
                TotalQuantitySold = 500 - (i * 10),
                TotalRevenue = 5000.00m - (i * 100)
            });
        }

        _mockRepository.Setup(r => r.GetProductPerformanceAsync(topN))
            .ReturnsAsync(productPerformance);

        var result = await _controller.GetProductPerformance(topN);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData(1000.00)]
    [InlineData(5000.00)]
    [InlineData(10000.00)]
    [InlineData(25000.00)]
    [InlineData(50000.00)]
    public async Task GetRevenueReport_WithDifferentRevenues_ReturnsData(decimal totalRevenue)
    {
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var revenueData = new RevenueReportResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = totalRevenue,
            TotalOrders = 100,
            AverageOrderValue = totalRevenue / 100,
            DailyBreakdown = new List<DailyRevenueDto>()
        };

        _mockRepository.Setup(r => r.GetRevenueReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(revenueData);

        var result = await _controller.GetRevenueReport(startDate, endDate);
        result.Should().BeOfType<OkObjectResult>();
    }
}

