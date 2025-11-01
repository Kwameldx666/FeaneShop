using FluentAssertions;
using ProductService.Domain.Entities;

namespace ProductService.Tests.Domain;

public class DishTests
{
    [Fact]
    public void Dish_Creation_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var dish = new Dish
        {
            Name = "Pizza Margherita",
            Description = "Classic Italian pizza",
            Price = 15.99m,
            Category = "italian",
            IsAvailable = true
        };

        // Assert
        dish.Name.Should().Be("Pizza Margherita");
        dish.Price.Should().Be(15.99m);
        dish.IsAvailable.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(10.00)]
    [InlineData(100.00)]
    [InlineData(999.99)]
    public void Dish_AcceptsValidPrices(decimal price)
    {
        // Arrange & Act
        var dish = new Dish
        {
            Name = "Test Dish",
            Price = price,
            Category = "test"
        };

        // Assert
        dish.Price.Should().Be(price);
    }

    [Theory]
    [InlineData("italian")]
    [InlineData("mexican")]
    [InlineData("chinese")]
    [InlineData("japanese")]
    public void Dish_AcceptsDifferentCategories(string category)
    {
        // Arrange & Act
        var dish = new Dish
        {
            Name = "Test",
            Price = 10m,
            Category = category
        };

        // Assert
        dish.Category.Should().Be(category);
    }

    [Fact]
    public void Dish_CanBeMarkedAsUnavailable()
    {
        // Arrange
        var dish = new Dish
        {
            Name = "Test",
            Price = 10m,
            IsAvailable = true
        };

        // Act
        dish.IsAvailable = false;

        // Assert
        dish.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Dish_UpdatedAt_CanBeSet()
    {
        // Arrange
        var dish = new Dish
        {
            Name = "Test",
            Price = 10m,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var updateTime = DateTime.UtcNow;
        dish.UpdatedAt = updateTime;

        // Assert
        dish.UpdatedAt.Should().BeCloseTo(updateTime, TimeSpan.FromSeconds(1));
    }
}

