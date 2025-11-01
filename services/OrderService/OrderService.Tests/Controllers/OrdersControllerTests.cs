using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Controllers;
using OrderService.Domain.Entities;
using System.Security.Claims;
using Xunit;

namespace OrderService.Tests.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly Mock<ILogger<OrdersController>> _mockLogger;
    private readonly OrdersController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public OrdersControllerTests()
    {
        _mockRepository = new Mock<IOrderRepository>();
        _mockLogger = new Mock<ILogger<OrdersController>>();
        _controller = new OrdersController(_mockRepository.Object, _mockLogger.Object);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetUserOrders_WithValidUser_ReturnsOrders()
    {
        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                TotalAmount = 100.50m,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockRepository
            .Setup(r => r.GetUserOrdersAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var result = await _controller.GetUserOrders(CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetOrderById_WithValidId_ReturnsOrder()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = _testUserId,
            TotalAmount = 150.00m,
            Status = OrderStatus.Preparing,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _controller.GetOrderById(orderId, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_CreatesOrder()
    {
        var request = new CreateOrderRequest
        {
            DeliveryAddress = "123 Test St",
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test",
                    UnitPrice = 50.00m,
                    Quantity = 2
                }
            }
        };

        var createdOrder = new Order
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            TotalAmount = 100.00m,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        var result = await _controller.CreateOrder(request, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }
}

