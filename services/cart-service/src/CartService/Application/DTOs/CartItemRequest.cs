using System.ComponentModel.DataAnnotations;

namespace CartService.Application.DTOs;

public class CartItemRequest
{
    [Required] public Guid ProductId { get; set; }

    [Required] [MaxLength(200)] public string ProductName { get; set; } = string.Empty;

    [MaxLength(512)] public string? ProductImageUrl { get; set; }

    [Range(0, double.MaxValue)] public decimal UnitPrice { get; set; }

    [Range(1, 100)] public int Quantity { get; set; } = 1;

    [MaxLength(1024)] public string? Notes { get; set; }
}