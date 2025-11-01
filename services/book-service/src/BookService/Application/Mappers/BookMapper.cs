using BookService.Application.DTOs;
using BookService.Domain.Entities;

namespace BookService.Application.Mappers;

public static class BookMapper
{
    public static BookResponse ToResponse(this Book book)
    {
        if (book == null) throw new ArgumentNullException(nameof(book));

        var coverUrl = string.IsNullOrWhiteSpace(book.CoverImageBase64)
            ? "/images/books/default.png"
            : $"data:{book.CoverImageMimeType ?? "image/png"};base64,{book.CoverImageBase64}";

        return new BookResponse(
            book.Id,
            book.Title,
            book.Author,
            book.Description,
            book.Price,
            book.Genre,
            book.Isbn,
            book.PublishedOn,
            coverUrl,
            book.IsAvailable,
            book.CreatedAt,
            book.UpdatedAt);
    }
}