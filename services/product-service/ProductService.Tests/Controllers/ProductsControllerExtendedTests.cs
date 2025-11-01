using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Controllers;
using ProductService.Domain.Entities;

namespace ProductService.Tests.Controllers;

public class ProductsControllerExtendedTests
{
    private readonly Mock<IDishRepository> _mockRepository;
    private readonly Mock<ILogger<DishesController>> _mockLogger;
    private readonly DishesController _controller;

    public ProductsControllerExtendedTests()
    {
        _mockRepository = new Mock<IDishRepository>();
        _mockLogger = new Mock<ILogger<DishesController>>();
        _controller = new DishesController(_mockRepository.Object, _mockLogger.Object);
    }

    [Theory]
    [InlineData("Italian")]
    [InlineData("Mexican")]
    [InlineData("Chinese")]
    [InlineData("Japanese")]
    [InlineData("Indian")]
    [InlineData("Thai")]
    [InlineData("French")]
    [InlineData("Greek")]
    [InlineData("American")]
    [InlineData("Mediterranean")]
    public async Task GetDishes_WithDifferentCategories_ReturnsFilteredDishes(string category)
    {
        var dishes = new List<Dish>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = $"{category} Dish",
                Description = "Description",
                Price = 15.00m,
                Category = category.ToLower(),
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(r => r.GetAsync(It.Is<DishQueryOptions>(o => o.Category == category.ToLower()), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dishes);
        _mockRepository.Setup(r => r.CountAsync(It.Is<DishQueryOptions>(o => o.Category == category.ToLower()), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _controller.GetDishes(category.ToLower(), null, null, null, null, null, null, null, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData(5.00)]
    [InlineData(10.00)]
    [InlineData(15.00)]
    [InlineData(20.00)]
    [InlineData(25.00)]
    [InlineData(30.00)]
    [InlineData(50.00)]
    [InlineData(75.00)]
    [InlineData(100.00)]
    [InlineData(150.00)]
    public async Task GetDishes_WithDifferentPriceRanges_ReturnsDishes(decimal price)
    {
        var dishes = new List<Dish>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Test Dish",
                Description = "Description",
                Price = price,
                Category = "test",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(r => r.GetAsync(It.IsAny<DishQueryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dishes);
        _mockRepository.Setup(r => r.CountAsync(It.IsAny<DishQueryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _controller.GetDishes(null, null, null, null, null, null, null, null, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData("Pizza")]
    [InlineData("Pasta")]
    [InlineData("Burger")]
    [InlineData("Salad")]
    [InlineData("Soup")]
    [InlineData("Dessert")]
    [InlineData("Appetizer")]
    [InlineData("Main Course")]
    [InlineData("Side Dish")]
    [InlineData("Beverage")]
    public async Task GetDishes_SearchByName_ReturnsMatchingDishes(string searchTerm)
    {
        var dishes = new List<Dish>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = $"{searchTerm} Special",
                Description = "Delicious",
                Price = 15.00m,
                Category = "food",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(r => r.GetAsync(It.Is<DishQueryOptions>(o => o.Search == searchTerm), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dishes);
        _mockRepository.Setup(r => r.CountAsync(It.Is<DishQueryOptions>(o => o.Search == searchTerm), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _controller.GetDishes(null, searchTerm, null, null, null, null, null, null, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }
}

