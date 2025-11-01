using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.DTOs;

public class DishUpsertRequest
{
    public Guid? Id { get; set; }

    [Required] [StringLength(128)] public string Name { get; set; } = string.Empty;

    [Required] [StringLength(1024)] public string Description { get; set; } = string.Empty;

    [Range(0.01, 1000)] public decimal Price { get; set; }

    [Required] [StringLength(64)] public string Category { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    public bool IsFeatured { get; set; }

    [Range(0, int.MaxValue)] public int PopularityScore { get; set; }

    public IFormFile? ImageFile { get; set; }
}