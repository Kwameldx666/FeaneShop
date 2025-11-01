using System;
using System.Collections.Generic;
using CartService.Application.DTOs;
using CartService.Application.Interfaces;
using CartService.Controllers;
using CartService.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CartService.Tests.Controllers;

public class CartControllerTests
{
    private readonly Mock<ICartRepository> _mockRepository;
    private readonly Mock<ILogger<CartController>> _mockLogger;
    private readonly CartController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public CartControllerTests()
    {
        _mockRepository = new Mock<ICartRepository>();
        _mockLogger = new Mock<ILogger<CartController>>();
        _controller = new CartController(_mockRepository.Object, _mockLogger.Object);

        // Setup authenticated user context
        var claims = new List<Claim>
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", _testUserId.ToString()),
            new Claim("sub", _testUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetCartItems_ReturnsUserCartItems()
    {
        // Arrange
        var cartItems = new List<CartItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                ProductId = Guid.NewGuid(),
                ProductName = "Pizza",
                Quantity = 2,
                UnitPrice = 15.00m
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                ProductId = Guid.NewGuid(),
                ProductName = "Burger",
                Quantity = 1,
                UnitPrice = 12.00m
            }
        };

        _mockRepository.Setup(r => r.GetItemsAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItems);

        // Act
        var result = await _controller.GetCartItems(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task AddItem_WithValidRequest_AddsItem()
    {
        // Arrange
        var request = new CartItemRequest
        {
            ProductId = Guid.NewGuid(),
            ProductName = "New Pizza",
            Quantity = 1,
            UnitPrice = 16.00m
        };

        var addedItem = new CartItem
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            ProductId = request.ProductId,
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice
        };

        _mockRepository.Setup(r => r.AddOrUpdateAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(addedItem);

        // Act
        var result = await _controller.AddItem(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateItem_WithValidData_UpdatesQuantity()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var request = new CartItemUpdateRequest
        {
            Quantity = 5
        };

        var updatedItem = new CartItem
        {
            Id = itemId,
            UserId = _testUserId,
            ProductId = Guid.NewGuid(),
            ProductName = "Pizza",
            Quantity = 5,
            UnitPrice = 15.00m
        };

        _mockRepository.Setup(r => r.UpdateAsync(_testUserId, itemId, It.IsAny<Action<CartItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockRepository.Setup(r => r.GetItemAsync(_testUserId, itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedItem);

        // Act
        var result = await _controller.UpdateItem(itemId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RemoveItem_WithValidId_RemovesItem()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        _mockRepository.Setup(r => r.RemoveAsync(_testUserId, itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveItem(itemId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ClearCart_RemovesAllUserItems()
    {
        // Arrange
        _mockRepository.Setup(r => r.ClearAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.ClearCart(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RemoveItem_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        _mockRepository.Setup(r => r.RemoveAsync(_testUserId, itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RemoveItem(itemId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateItem_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var request = new CartItemUpdateRequest { Quantity = 5 };

        _mockRepository.Setup(r => r.UpdateAsync(_testUserId, itemId, It.IsAny<Action<CartItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateItem(itemId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
