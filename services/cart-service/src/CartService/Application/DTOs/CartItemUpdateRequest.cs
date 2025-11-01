using System.ComponentModel.DataAnnotations;

namespace CartService.Application.DTOs;

public class CartItemUpdateRequest
{
    [Range(1, 100)] public int? Quantity { get; set; }

    [MaxLength(1024)] public string? Notes { get; set; }
}