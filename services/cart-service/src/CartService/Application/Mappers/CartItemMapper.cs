using CartService.Application.DTOs;
using CartService.Domain.Entities;

namespace CartService.Application.Mappers;

public static class CartItemMapper
{
    public static CartItemResponse ToResponse(CartItem item)
    {
        return new CartItemResponse
        {
            Id = item.Id,
            UserId = item.UserId,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            ProductImageUrl = item.ProductImageUrl,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            TotalPrice = item.TotalPrice,
            Notes = item.Notes,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }

    public static void ApplyCreate(this CartItem item, CartItemRequest request)
    {
        item.ProductId = request.ProductId;
        item.ProductName = request.ProductName.Trim();
        item.ProductImageUrl = string.IsNullOrWhiteSpace(request.ProductImageUrl)
            ? null
            : request.ProductImageUrl.Trim();
        item.UnitPrice = Math.Round(request.UnitPrice, 2, MidpointRounding.AwayFromZero);
        item.Quantity = Math.Clamp(request.Quantity, 1, 100);
        item.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        item.UpdatedAt = DateTime.UtcNow;
    }
}