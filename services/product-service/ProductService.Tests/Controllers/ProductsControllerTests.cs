using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Controllers;
using ProductService.Domain.Entities;
using Xunit;

namespace ProductService.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IDishRepository> _mockRepository;
    private readonly Mock<ILogger<DishesController>> _mockLogger;
    private readonly DishesController _controller;

    public ProductsControllerTests()
    {
        _mockRepository = new Mock<IDishRepository>();
        _mockLogger = new Mock<ILogger<DishesController>>();
        _controller = new DishesController(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetDishes_ReturnsAllDishes()
    {
        // Arrange
        var dishes = new List<Dish>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Pizza Margherita",
                Description = "Classic pizza",
                Price = 15.00m,
                Category = "Pizza",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Burger Deluxe",
                Description = "Delicious burger",
                Price = 12.50m,
                Category = "Burgers",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(r => r.GetAsync(It.IsAny<DishQueryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dishes);
        _mockRepository.Setup(r => r.CountAsync(It.IsAny<DishQueryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.GetDishes(null, null, null, null, null, null, null, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDish_WithValidId_ReturnsDish()
    {
        // Arrange
        var dishId = Guid.NewGuid();
        var dish = new Dish
        {
            Id = dishId,
            Name = "Caesar Salad",
            Description = "Fresh salad",
            Price = 10.00m,
            Category = "Salads",
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(dishId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dish);

        // Act
        var result = await _controller.GetDish(dishId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDish_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(invalidId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Dish?)null);

        // Act
        var result = await _controller.GetDish(invalidId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetDishes_WithCategoryFilter_ReturnsFilteredDishes()
    {
        // Arrange
        var dishes = new List<Dish>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Pizza 1",
                Description = "Pizza description",
                Price = 15.00m,
                Category = "Pizza",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Pizza 2",
                Description = "Another pizza",
                Price = 16.00m,
                Category = "Pizza",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(r => r.GetAsync(It.Is<DishQueryOptions>(o => o.Category == "Pizza"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dishes);
        _mockRepository.Setup(r => r.CountAsync(It.Is<DishQueryOptions>(o => o.Category == "Pizza"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.GetDishes("Pizza", null, null, null, null, null, null, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
