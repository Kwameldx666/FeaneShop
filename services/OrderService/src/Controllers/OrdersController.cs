using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappers;
using OrderService.Domain.Entities;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly IOrderRepository _repository;

    public OrdersController(IOrderRepository repository, ILogger<OrdersController> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetUserOrders(CancellationToken cancellationToken)
    {
        var userId = ExtractUserId();
        if (!userId.HasValue)
        {
            _logger.LogWarning("GetUserOrders: Unable to extract userId");
            return Unauthorized(new { success = false, message = "User context is required." });
        }

        _logger.LogInformation("GetUserOrders: Loading orders for user {UserId}", userId.Value);

        var orders = await _repository.GetUserOrdersAsync(userId.Value, cancellationToken);
        var responses = orders.Select(OrderMapper.ToResponse).ToList();

        _logger.LogInformation("GetUserOrders returned {Count} orders for user {UserId}", responses.Count,
            userId.Value);

        return Ok(new
        {
            success = true,
            orders = responses
        });
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetOrderById(Guid orderId, CancellationToken cancellationToken)
    {
        var userId = ExtractUserId();
        if (!userId.HasValue) return Unauthorized(new { success = false, message = "User context is required." });

        var order = await _repository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            return NotFound(new { success = false, message = "Order not found." });
        }

        if (order.UserId != userId.Value)
        {
            _logger.LogWarning("User {UserId} attempted to access order {OrderId} owned by {OwnerId}",
                userId.Value, orderId, order.UserId);
            return Forbid();
        }

        return Ok(new
        {
            success = true,
            order = OrderMapper.ToResponse(order)
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("CreateOrder called with {ItemCount} items", request.Items?.Count ?? 0);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("CreateOrder validation failed: {@ModelState}", ModelState);
            return BadRequest(new { success = false, message = "Invalid request data.", errors = ModelState });
        }

        var userId = ExtractUserId();
        if (!userId.HasValue)
        {
            _logger.LogWarning("CreateOrder: User context not found");
            return Unauthorized(new { success = false, message = "User context is required." });
        }

        var userName = ExtractUserName();
        var userEmail = ExtractUserEmail();

        var order = new Order
        {
            UserId = userId.Value,
            UserName = userName,
            UserEmail = userEmail,
            DeliveryAddress = request.DeliveryAddress,
            Notes = request.Notes,
            Status = OrderStatus.Pending,
            Items = (request.Items ?? new List<OrderItemDto>())
                .Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductImageUrl = item.ProductImageUrl,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    Notes = item.Notes
                }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

        var created = await _repository.CreateAsync(order, cancellationToken);

        _logger.LogInformation("Order {OrderId} created for user {UserId}, total: {Total}",
            created.Id, userId.Value, created.TotalAmount);

        return Ok(new
        {
            success = true,
            order = OrderMapper.ToResponse(created),
            message = "Order created successfully."
        });
    }

    [HttpPatch("{orderId:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Invalid request data." });

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
            return BadRequest(new { success = false, message = "Invalid order status." });

        var updated = await _repository.UpdateStatusAsync(orderId, status, cancellationToken);
        if (!updated) return NotFound(new { success = false, message = "Order not found." });

        _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, status);

        return Ok(new
        {
            success = true,
            message = $"Order status updated to {status}."
        });
    }

    [HttpDelete("{orderId:guid}")]
    public async Task<IActionResult> CancelOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var userId = ExtractUserId();
        if (!userId.HasValue) return Unauthorized(new { success = false, message = "User context is required." });

        var order = await _repository.GetByIdAsync(orderId, cancellationToken);
        if (order == null) return NotFound(new { success = false, message = "Order not found." });

        if (order.UserId != userId.Value) return Forbid();

        var cancelled = await _repository.CancelOrderAsync(orderId, cancellationToken);
        if (!cancelled) return BadRequest(new { success = false, message = "Order cannot be cancelled." });

        _logger.LogInformation("Order {OrderId} cancelled by user {UserId}", orderId, userId.Value);

        return Ok(new
        {
            success = true,
            message = "Order cancelled successfully."
        });
    }

    private Guid? ExtractUserId()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            var claim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                        ?? User.FindFirst("sub")
                        ?? User.FindFirst("user_id")
                        ?? User.FindFirst("userId");

            if (claim != null && Guid.TryParse(claim.Value, out var userId)) return userId;
        }

        return null;
    }

    private string? ExtractUserName()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            var claim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                        ?? User.FindFirst("name");
            return claim?.Value;
        }

        return null;
    }

    private string? ExtractUserEmail()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            var claim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                        ?? User.FindFirst("email");
            return claim?.Value;
        }

        return null;
    }
}
