using System.ComponentModel.DataAnnotations;

namespace OrderService.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    [MaxLength(200)] public string ProductName { get; set; } = string.Empty;

    [MaxLength(512)] public string? ProductImageUrl { get; set; }

    [Range(0, double.MaxValue)] public decimal UnitPrice { get; set; }

    [Range(1, 100)] public int Quantity { get; set; } = 1;

    [MaxLength(1024)] public string? Notes { get; set; }

    public decimal TotalPrice => Math.Round(UnitPrice * Quantity, 2, MidpointRounding.AwayFromZero);

    public Order? Order { get; set; }
}