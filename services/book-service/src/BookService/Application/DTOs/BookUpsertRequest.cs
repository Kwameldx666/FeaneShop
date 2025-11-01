using System.ComponentModel.DataAnnotations;

namespace BookService.Application.DTOs;

public class BookUpsertRequest
{
    public Guid? Id { get; set; }

    [Required] [StringLength(256)] public string Title { get; set; } = string.Empty;

    [Required] [StringLength(128)] public string Author { get; set; } = string.Empty;

    [Required] [StringLength(4096)] public string Description { get; set; } = string.Empty;

    [Range(0.01, 2000)] public decimal Price { get; set; }

    [Required] [StringLength(64)] public string Genre { get; set; } = string.Empty;

    [StringLength(32)] public string? Isbn { get; set; }

    public DateTime? PublishedOn { get; set; }

    public bool IsAvailable { get; set; } = true;

    public IFormFile? CoverImage { get; set; }
}