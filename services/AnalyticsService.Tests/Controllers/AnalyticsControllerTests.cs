using AnalyticsService.Controllers;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsService.Tests.Controllers;

public class AnalyticsControllerTests : IDisposable
{
    private readonly AnalyticsDbContext _context;
    private readonly AnalyticsController _controller;
    private readonly Mock<ILogger<AnalyticsController>> _mockLogger;

    public AnalyticsControllerTests()
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AnalyticsDbContext(options);
        _mockLogger = new Mock<ILogger<AnalyticsController>>();
        _controller = new AnalyticsController(_context, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task GetDashboard_ReturnsCorrectMetrics()
    {
        // Arrange
        var events = new List<AnalyticsEvent>
        {
            new()
            {
                Id = Guid.NewGuid(),
                EventType = "OrderCompleted",
                OrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = "Pizza",
                Quantity = 2,
                Amount = 30.00m,
                Timestamp = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventType = "OrderCompleted",
                OrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = "Burger",
                Quantity = 1,
                Amount = 15.00m,
                Timestamp = DateTime.UtcNow.AddDays(-2)
            }
        };

        _context.AnalyticsEvents.AddRange(events);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetDashboard(
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        // Assert
        result.Should().NotBeNull();
        result.TotalRevenue.Should().Be(45.00m);
        result.TotalOrders.Should().Be(2);
        result.TopProducts.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetRevenue_WithDateRange_ReturnsCorrectData()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var events = new List<AnalyticsEvent>
        {
            new()
            {
                Id = Guid.NewGuid(),
                EventType = "OrderCompleted",
                OrderId = Guid.NewGuid(),
                Amount = 100.00m,
                Timestamp = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventType = "OrderCompleted",
                OrderId = Guid.NewGuid(),
                Amount = 200.00m,
                Timestamp = DateTime.UtcNow.AddDays(-3)
            }
        };

        _context.AnalyticsEvents.AddRange(events);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetRevenue(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task RecordEvent_WithValidData_CreatesEvent()
    {
        // Arrange
        var newEvent = new AnalyticsEvent
        {
            EventType = "OrderCompleted",
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Caesar Salad",
            Quantity = 3,
            Amount = 45.00m,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _controller.RecordEvent(newEvent);

        // Assert
        result.Should().NotBeNull();
        var createdEvent = await _context.AnalyticsEvents
            .FirstOrDefaultAsync(e => e.ProductName == "Caesar Salad");

        createdEvent.Should().NotBeNull();
        createdEvent!.Quantity.Should().Be(3);
        createdEvent.Amount.Should().Be(45.00m);
    }

    [Fact]
    public async Task GetTopProducts_ReturnsCorrectRanking()
    {
        // Arrange
        var events = new List<AnalyticsEvent>
        {
            new()
            {
                Id = Guid.NewGuid(),
                EventType = "OrderCompleted",
                ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ProductName = "Pizza",
                Quantity = 10,
                Amount = 150.00m,
                Timestamp = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventType = "OrderCompleted",
                ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ProductName = "Pizza",
                Quantity = 5,
                Amount = 75.00m,
                Timestamp = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventType = "OrderCompleted",
                ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ProductName = "Burger",
                Quantity = 8,
                Amount = 120.00m,
                Timestamp = DateTime.UtcNow
            }
        };

        _context.AnalyticsEvents.AddRange(events);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTopProducts(
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            5);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
        var topProduct = result.First();
        topProduct.ProductName.Should().Be("Pizza");
        topProduct.TotalRevenue.Should().Be(225.00m);
        topProduct.UnitsSold.Should().Be(15);
    }

    [Fact]
    public async Task GetDashboard_WithNoData_ReturnsEmptyMetrics()
    {
        // Arrange
        // Пустая база данных

        // Act
        var result = await _controller.GetDashboard(
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        // Assert
        result.Should().NotBeNull();
        result.TotalRevenue.Should().Be(0);
        result.TotalOrders.Should().Be(0);
        result.TopProducts.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(30)]
    public async Task GetDashboard_WithDifferentDateRanges_ReturnsCorrectData(int days)
    {
        // Arrange
        var events = Enumerable.Range(0, days)
            .Select(i => new AnalyticsEvent
            {
                Id = Guid.NewGuid(),
                EventType = "OrderCompleted",
                OrderId = Guid.NewGuid(),
                Amount = 50.00m,
                Timestamp = DateTime.UtcNow.AddDays(-i)
            })
            .ToList();

        _context.AnalyticsEvents.AddRange(events);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetDashboard(
            DateTime.UtcNow.AddDays(-days),
            DateTime.UtcNow);

        // Assert
        result.Should().NotBeNull();
        result.TotalOrders.Should().Be(days);
        result.TotalRevenue.Should().Be(50.00m * days);
    }

    [Fact]
    public async Task GetAverageOrderValue_CalculatesCorrectly()
    {
        // Arrange
        var events = new List<AnalyticsEvent>
        {
            new()
            {
                Id = Guid.NewGuid(),
                EventType = "OrderCompleted",
                OrderId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Amount = 100.00m,
                Timestamp = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventType = "OrderCompleted",
                OrderId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Amount = 200.00m,
                Timestamp = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventType = "OrderCompleted",
                OrderId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Amount = 150.00m,
                Timestamp = DateTime.UtcNow
            }
        };

        _context.AnalyticsEvents.AddRange(events);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetDashboard(
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow);

        // Assert
        result.AverageOrderValue.Should().Be(150.00m); // (100 + 200 + 150) / 3
    }
}