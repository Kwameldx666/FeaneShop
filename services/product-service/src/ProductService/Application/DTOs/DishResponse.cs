namespace ProductService.Application.DTOs;

public record DishResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    string ImageUrl,
    bool IsAvailable,
    bool IsFeatured,
    int PopularityScore,
    DateTime CreatedAt,
    DateTime UpdatedAt);
