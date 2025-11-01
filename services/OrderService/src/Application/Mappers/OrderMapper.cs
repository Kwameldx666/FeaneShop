using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mappers;

public static class OrderMapper
{
    public static OrderResponse ToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            UserName = order.UserName,
            UserEmail = order.UserEmail,
            UserPhone = order.UserPhone,
            DeliveryAddress = order.DeliveryAddress,
            Notes = order.Notes,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(ToItemResponse).ToList()
        };
    }

    public static OrderItemResponse ToItemResponse(OrderItem item)
    {
        return new OrderItemResponse
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            ProductImageUrl = item.ProductImageUrl,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            Notes = item.Notes,
            TotalPrice = item.TotalPrice
        };
    }
}