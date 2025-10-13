using System.ComponentModel.DataAnnotations;

namespace Feane.Contracts.Dishes;

public class CreateDishRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(300)]
    public string? ImageUrl { get; set; }
}
