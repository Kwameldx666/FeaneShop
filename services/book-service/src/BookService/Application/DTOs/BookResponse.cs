namespace BookService.Application.DTOs;

public record BookResponse(
    Guid Id,
    string Title,
    string Author,
    string Description,
    decimal Price,
    string Genre,
    string? Isbn,
    DateTime? PublishedOn,
    string CoverImageUrl,
    bool IsAvailable,
    DateTime CreatedAt,
    DateTime UpdatedAt);