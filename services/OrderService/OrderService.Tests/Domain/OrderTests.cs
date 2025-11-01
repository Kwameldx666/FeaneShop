using FluentAssertions;
using OrderService.Domain.Entities;

namespace OrderService.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void Order_Creation_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            TotalAmount = 100.00m,
            DeliveryAddress = "123 Test St",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        order.UserId.Should().NotBeEmpty();
        order.Status.Should().Be(OrderStatus.Pending);
        order.TotalAmount.Should().Be(100.00m);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Ready)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Order_CanHaveDifferentStatuses(OrderStatus status)
    {
        // Arrange & Act
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            Status = status,
            TotalAmount = 50.00m
        };

        // Assert
        order.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(10.00)]
    [InlineData(50.00)]
    [InlineData(100.00)]
    [InlineData(500.00)]
    [InlineData(1000.00)]
    public void Order_AcceptsDifferentTotalAmounts(decimal amount)
    {
        // Arrange & Act
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            TotalAmount = amount
        };

        // Assert
        order.TotalAmount.Should().Be(amount);
    }

    [Fact]
    public void Order_CanHaveDeliveryAddress()
    {
        // Arrange & Act
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            TotalAmount = 100.00m,
            DeliveryAddress = "456 Main St, Apt 2B"
        };

        // Assert
        order.DeliveryAddress.Should().Be("456 Main St, Apt 2B");
    }

    [Fact]
    public void Order_CanHaveNotes()
    {
        // Arrange & Act
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            TotalAmount = 100.00m,
            Notes = "Please ring doorbell twice"
        };

        // Assert
        order.Notes.Should().Be("Please ring doorbell twice");
    }

    [Fact]
    public void Order_TracksCreationTime()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        
        // Act
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            TotalAmount = 100.00m,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        order.CreatedAt.Should().BeOnOrAfter(beforeCreate);
    }

    [Fact]
    public void Order_TracksUpdateTime()
    {
        // Arrange
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            TotalAmount = 100.00m,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var updateTime = DateTime.UtcNow.AddMinutes(5);
        order.UpdatedAt = updateTime;

        // Assert
        order.UpdatedAt.Should().BeCloseTo(updateTime, TimeSpan.FromSeconds(1));
    }
}

