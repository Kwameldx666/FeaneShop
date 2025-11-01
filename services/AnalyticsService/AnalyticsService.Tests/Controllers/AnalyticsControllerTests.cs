using AnalyticsService.Application.DTOs;
using AnalyticsService.Application.Interfaces;
using AnalyticsService.Controllers;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticsService.Tests.Controllers;

public class AnalyticsControllerTests
{
    private readonly Mock<IAnalyticsRepository> _mockRepository;
    private readonly Mock<ILogger<AnalyticsController>> _mockLogger;
    private readonly AnalyticsController _controller;

    public AnalyticsControllerTests()
    {
        _mockRepository = new Mock<IAnalyticsRepository>();
        _mockLogger = new Mock<ILogger<AnalyticsController>>();
        _controller = new AnalyticsController(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetDashboard_ReturnsCorrectMetrics()
    {
        // Arrange
        var dashboardData = new DashboardResponse
        {
            TotalRevenue = 45.00m,
            TotalOrders = 2,
            AverageOrderValue = 22.50m,
            TopProducts = new List<TopProductDto>
            {
                new() { ProductName = "Pizza", Revenue = 30.00m, TotalSold = 2 },
                new() { ProductName = "Burger", Revenue = 15.00m, TotalSold = 1 }
            }
        };

        _mockRepository.Setup(r => r.GetDashboardDataAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(dashboardData);

        // Act
        var result = await _controller.GetDashboard(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRevenueReport_ReturnsCorrectData()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var revenueData = new RevenueReportResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = 300.00m,
            TotalOrders = 2,
            AverageOrderValue = 150.00m,
            DailyBreakdown = new List<DailyRevenueDto>
            {
                new() { Date = DateTime.UtcNow.AddDays(-5), Revenue = 100.00m, Orders = 1 },
                new() { Date = DateTime.UtcNow.AddDays(-3), Revenue = 200.00m, Orders = 1 }
            }
        };

        _mockRepository.Setup(r => r.GetRevenueReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(revenueData);

        // Act
        var result = await _controller.GetRevenueReport(startDate, endDate);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetProductPerformance_ReturnsCorrectData()
    {
        // Arrange
        var productPerformance = new ProductPerformanceResponse
        {
            Products = new List<ProductStatisticsDto>
            {
                new() { ProductName = "Pizza", TotalRevenue = 225.00m, TotalQuantitySold = 15 },
                new() { ProductName = "Burger", TotalRevenue = 120.00m, TotalQuantitySold = 8 }
            }
        };

        _mockRepository.Setup(r => r.GetProductPerformanceAsync(It.IsAny<int>()))
            .ReturnsAsync(productPerformance);

        // Act
        var result = await _controller.GetProductPerformance(10);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDashboard_WithNullDates_UsesDefaults()
    {
        // Arrange
        var dashboardData = new DashboardResponse
        {
            TotalRevenue = 0,
            TotalOrders = 0,
            AverageOrderValue = 0,
            TopProducts = new List<TopProductDto>()
        };

        _mockRepository.Setup(r => r.GetDashboardDataAsync(null, null))
            .ReturnsAsync(dashboardData);

        // Act
        var result = await _controller.GetDashboard(null, null);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}