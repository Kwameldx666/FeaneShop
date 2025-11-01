using System.ComponentModel.DataAnnotations;

namespace CartService.Domain.Entities;

public class CartItem
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }

    [MaxLength(200)] public string ProductName { get; set; } = string.Empty;

    [MaxLength(512)] public string? ProductImageUrl { get; set; }

    [Range(0, double.MaxValue)] public decimal UnitPrice { get; set; }

    [Range(1, 100)] public int Quantity { get; set; } = 1;

    [MaxLength(1024)] public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public decimal TotalPrice => Math.Round(UnitPrice * Quantity, 2, MidpointRounding.AwayFromZero);
}