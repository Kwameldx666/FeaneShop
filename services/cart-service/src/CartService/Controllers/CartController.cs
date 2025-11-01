using CartService.Application.DTOs;
using CartService.Application.Interfaces;
using CartService.Application.Mappers;
using CartService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ILogger<CartController> _logger;
    private readonly ICartRepository _repository;

    public CartController(ICartRepository repository, ILogger<CartController> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetCartItems(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetCartItems called, User.Identity.IsAuthenticated: {IsAuth}",
            User?.Identity?.IsAuthenticated);

        var userId = ExtractUserId();
        if (!userId.HasValue)
        {
            _logger.LogWarning("GetCartItems: Unable to extract userId from claims");
            return Unauthorized(new { success = false, message = "User context is required." });
        }

        _logger.LogInformation("GetCartItems: Loading items for user {UserId}", userId.Value);

        var items = await _repository.GetItemsAsync(userId.Value, cancellationToken);
        var responses = items.Select(CartItemMapper.ToResponse).ToList();
        var total = responses.Sum(i => i.TotalPrice);
        _logger.LogInformation("GetCartItems returned {Count} items for user {UserId}, total: {Total}", responses.Count,
            userId.Value, total);

        return Ok(new
        {
            success = true,
            items = responses,
            totalPrice = Math.Round(total, 2, MidpointRounding.AwayFromZero)
        });
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] CartItemRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AddItem called with request: {@Request}", request);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("AddItem validation failed: {@ModelState}", ModelState);
            return BadRequest(new { success = false, message = "Invalid request data.", errors = ModelState });
        }

        var userId = ExtractUserId();
        if (!userId.HasValue)
        {
            _logger.LogWarning("AddItem: User context not found");
            return Unauthorized(new { success = false, message = "User context is required." });
        }

        var entity = new CartItem
        {
            UserId = userId.Value
        };

        entity.ApplyCreate(request);
        entity.Quantity = ClampQuantity(entity.Quantity);

        var result = await _repository.AddOrUpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Added or updated product {ProductId} for user {UserId}", request.ProductId,
            userId.Value);
        return Ok(new
        {
            success = true,
            item = CartItemMapper.ToResponse(result)
        });
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] CartItemRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AddToCart called with request: {@Request}", request);
        return await AddItem(request, cancellationToken);
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] CartItemUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId();
        if (!userId.HasValue) return Unauthorized(new { success = false, message = "User context is required." });

        var updated = await _repository.UpdateAsync(userId.Value, id, item =>
        {
            if (request.Quantity.HasValue) item.Quantity = ClampQuantity(request.Quantity.Value);

            if (request.Notes is not null)
                item.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        }, cancellationToken);

        if (!updated) return NotFound(new { success = false, message = "Cart item not found." });

        var item = await _repository.GetItemAsync(userId.Value, id, cancellationToken);
        _logger.LogInformation("Updated cart item {CartItemId} for user {UserId}", id, userId.Value);
        return Ok(new
        {
            success = true,
            item = item is null ? null : CartItemMapper.ToResponse(item)
        });
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id, CancellationToken cancellationToken)
    {
        var userId = ExtractUserId();
        if (!userId.HasValue) return Unauthorized(new { success = false, message = "User context is required." });

        var removed = await _repository.RemoveAsync(userId.Value, id, cancellationToken);
        if (!removed) return NotFound(new { success = false, message = "Cart item not found." });

        _logger.LogInformation("Removed cart item {CartItemId} for user {UserId}", id, userId.Value);
        return Ok(new { success = true, message = "Item removed." });
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
    {
        var userId = ExtractUserId();
        if (!userId.HasValue) return Unauthorized(new { success = false, message = "User context is required." });

        var removed = await _repository.ClearAsync(userId.Value, cancellationToken);
        _logger.LogInformation("Cleared {Removed} cart items for user {UserId}", removed, userId.Value);
        return Ok(new { success = true, removed });
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

        if (Request.Headers.TryGetValue("X-User-Id", out var header) &&
            Guid.TryParse(header.ToString(), out var headerId)) return headerId;

        return null;
    }

    private static int ClampQuantity(int quantity)
    {
        return Math.Clamp(quantity, 1, 100);
    }
}