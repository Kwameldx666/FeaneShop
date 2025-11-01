using FluentAssertions;
using AnalyticsService.Application.DTOs;

namespace AnalyticsService.Tests.DTOs;

public class DashboardResponseTests
{
    [Fact]
    public void DashboardResponse_Creation_InitializesCollections()
    {
        // Arrange & Act
        var response = new DashboardResponse();

        // Assert
        response.TopProducts.Should().NotBeNull();
        response.TopProducts.Should().BeEmpty();
        response.RevenueChart.Should().NotBeNull();
        response.RevenueChart.Should().BeEmpty();
    }

    [Fact]
    public void DashboardResponse_CanSetMetrics()
    {
        // Arrange & Act
        var response = new DashboardResponse
        {
            TotalRevenue = 10000.00m,
            TotalOrders = 150,
            CompletedOrders = 140,
            CancelledOrders = 10,
            AverageOrderValue = 66.67m
        };

        // Assert
        response.TotalRevenue.Should().Be(10000.00m);
        response.TotalOrders.Should().Be(150);
        response.AverageOrderValue.Should().Be(66.67m);
    }

    [Fact]
    public void DashboardResponse_CanAddTopProducts()
    {
        // Arrange
        var response = new DashboardResponse();
        var topProduct = new TopProductDto
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Popular Item",
            TotalSold = 50,
            Revenue = 500.00m
        };

        // Act
        response.TopProducts.Add(topProduct);

        // Assert
        response.TopProducts.Should().HaveCount(1);
        response.TopProducts.First().ProductName.Should().Be("Popular Item");
    }
}

public class RevenueReportResponseTests
{
    [Fact]
    public void RevenueReportResponse_SetsDateRange()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Act
        var response = new RevenueReportResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = 5000.00m,
            TotalOrders = 100,
            AverageOrderValue = 50.00m
        };

        // Assert
        response.StartDate.Should().Be(startDate);
        response.EndDate.Should().Be(endDate);
    }

    [Theory]
    [InlineData(1000, 20, 50.00)]
    [InlineData(5000, 100, 50.00)]
    [InlineData(10000, 200, 50.00)]
    public void RevenueReportResponse_CalculatesAverageOrderValue(decimal total, int orders, decimal expected)
    {
        // Arrange & Act
        var response = new RevenueReportResponse
        {
            TotalRevenue = total,
            TotalOrders = orders,
            AverageOrderValue = total / orders
        };

        // Assert
        response.AverageOrderValue.Should().Be(expected);
    }
}

public class ProductStatisticsDtoTests
{
    [Fact]
    public void ProductStatisticsDto_SetsAllProperties()
    {
        // Arrange & Act
        var dto = new ProductStatisticsDto
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            TotalOrders = 50,
            TotalQuantitySold = 150,
            TotalRevenue = 1500.00m
        };

        // Assert
        dto.ProductName.Should().Be("Test Product");
        dto.TotalOrders.Should().Be(50);
        dto.TotalQuantitySold.Should().Be(150);
        dto.TotalRevenue.Should().Be(1500.00m);
    }

    [Theory]
    [InlineData(10, 30, 300.00)]
    [InlineData(25, 75, 750.00)]
    [InlineData(50, 150, 1500.00)]
    public void ProductStatisticsDto_AcceptsDifferentValues(int orders, int quantity, decimal revenue)
    {
        // Arrange & Act
        var dto = new ProductStatisticsDto
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Product",
            TotalOrders = orders,
            TotalQuantitySold = quantity,
            TotalRevenue = revenue
        };

        // Assert
        dto.TotalOrders.Should().Be(orders);
        dto.TotalQuantitySold.Should().Be(quantity);
        dto.TotalRevenue.Should().Be(revenue);
    }
}

public class DailyRevenueDtoTests
{
    [Fact]
    public void DailyRevenueDto_SetsProperties()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;

        // Act
        var dto = new DailyRevenueDto
        {
            Date = date,
            Revenue = 500.00m,
            Orders = 25
        };

        // Assert
        dto.Date.Should().Be(date);
        dto.Revenue.Should().Be(500.00m);
        dto.Orders.Should().Be(25);
    }

    [Theory]
    [InlineData(100.00, 5)]
    [InlineData(500.00, 25)]
    [InlineData(1000.00, 50)]
    public void DailyRevenueDto_AcceptsDifferentValues(decimal revenue, int orders)
    {
        // Arrange & Act
        var dto = new DailyRevenueDto
        {
            Date = DateTime.UtcNow.Date,
            Revenue = revenue,
            Orders = orders
        };

        // Assert
        dto.Revenue.Should().Be(revenue);
        dto.Orders.Should().Be(orders);
    }
}

