using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs;

public class CreateOrderRequest
{
    [MaxLength(500)] public string? DeliveryAddress { get; set; }

    [MaxLength(2000)] public string? Notes { get; set; }

    [Required] [MinLength(1)] public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    [Required] public Guid ProductId { get; set; }

    [Required] [MaxLength(200)] public string ProductName { get; set; } = string.Empty;

    [MaxLength(512)] public string? ProductImageUrl { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Required] [Range(1, 100)] public int Quantity { get; set; }

    [MaxLength(1024)] public string? Notes { get; set; }
}