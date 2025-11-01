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

public class CartControllerExtendedTests
{
    private readonly Mock<ICartRepository> _mockRepository;
    private readonly Mock<ILogger<CartController>> _mockLogger;
    private readonly CartController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public CartControllerExtendedTests()
    {
        _mockRepository = new Mock<ICartRepository>();
        _mockLogger = new Mock<ILogger<CartController>>();
        _controller = new CartController(_mockRepository.Object, _mockLogger.Object);

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

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    [InlineData(25)]
    [InlineData(30)]
    [InlineData(50)]
    public async Task AddItem_WithDifferentQuantities_AddsItem(int quantity)
    {
        var request = new CartItemRequest
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            Quantity = quantity,
            UnitPrice = 10.00m
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

        var result = await _controller.AddItem(request, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData(5.00)]
    [InlineData(10.00)]
    [InlineData(15.50)]
    [InlineData(20.00)]
    [InlineData(25.99)]
    [InlineData(50.00)]
    [InlineData(75.00)]
    [InlineData(100.00)]
    [InlineData(150.00)]
    [InlineData(200.00)]
    public async Task AddItem_WithDifferentPrices_AddsItem(decimal price)
    {
        var request = new CartItemRequest
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Product",
            Quantity = 1,
            UnitPrice = price
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

        var result = await _controller.AddItem(request, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData("Pizza Margherita")]
    [InlineData("Burger Deluxe")]
    [InlineData("Caesar Salad")]
    [InlineData("Pasta Carbonara")]
    [InlineData("Steak")]
    [InlineData("Chicken Wings")]
    [InlineData("Fish and Chips")]
    [InlineData("Veggie Bowl")]
    [InlineData("Sushi Roll")]
    [InlineData("Tacos")]
    public async Task AddItem_WithDifferentProductNames_AddsItem(string productName)
    {
        var request = new CartItemRequest
        {
            ProductId = Guid.NewGuid(),
            ProductName = productName,
            Quantity = 1,
            UnitPrice = 15.00m
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

        var result = await _controller.AddItem(request, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }
}

