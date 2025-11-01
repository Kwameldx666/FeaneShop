using BookService.Application.DTOs;
using BookService.Application.Interfaces;
using BookService.Application.Mappers;
using BookService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;
    private const int MaxImageSizeBytes = 2 * 1024 * 1024;
    private readonly ILogger<BooksController> _logger;

    private readonly IBookRepository _repository;

    public BooksController(IBookRepository repository, ILogger<BooksController> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks(
        [FromQuery] string? genre,
        [FromQuery] string? search,
        [FromQuery] bool? availableOnly,
        [FromQuery] string? sort,
        [FromQuery] bool? desc,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var options = new BookQueryOptions
        {
            Genre = genre,
            Search = search,
            AvailableOnly = availableOnly ?? false,
            SortBy = ParseSortField(sort),
            Descending = desc ?? ShouldSortDescending(sort),
            Limit = limit > 0 ? limit : null,
            Page = page is > 0 ? page : null,
            PageSize = pageSize is > 0 ? Math.Min(pageSize.Value, MaxPageSize) : DefaultPageSize
        };

        var books = await _repository.GetAsync(options, cancellationToken);
        var total = await _repository.CountAsync(options, cancellationToken);
        var responses = books.Select(BookMapper.ToResponse).ToList();

        return Ok(new
        {
            items = responses,
            totalCount = total,
            page = options.Page ?? 1,
            pageSize = options.PageSize ?? DefaultPageSize
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBook(Guid id, CancellationToken cancellationToken)
    {
        var book = await _repository.GetByIdAsync(id, cancellationToken);
        if (book == null) return NotFound(new { success = false, message = "Book not found." });

        return Ok(book.ToResponse());
    }

    [HttpGet("genres")]
    public async Task<IActionResult> GetGenres(CancellationToken cancellationToken)
    {
        var genres = await _repository.GetGenresAsync(cancellationToken);
        return Ok(new { items = genres });
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateBook([FromForm] BookUpsertRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var book = new Book
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            Description = request.Description.Trim(),
            Price = Math.Round(request.Price, 2, MidpointRounding.AwayFromZero),
            Genre = request.Genre.Trim().ToLowerInvariant(),
            Isbn = string.IsNullOrWhiteSpace(request.Isbn) ? null : request.Isbn.Trim(),
            PublishedOn = request.PublishedOn,
            IsAvailable = request.IsAvailable
        };

        try
        {
            (book.CoverImageBase64, book.CoverImageMimeType) =
                await ReadCoverAsync(request.CoverImage, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cover validation failed while creating a book.");
            return BadRequest(new { success = false, message = ex.Message });
        }

        var created = await _repository.AddAsync(book, cancellationToken);
        var response = created.ToResponse();

        return CreatedAtAction(nameof(GetBook), new { id = response.Id }, new
        {
            success = true,
            message = "Book created successfully.",
            item = response
        });
    }

    [HttpPost("{id:guid}")]
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateBook(Guid id, [FromForm] BookUpsertRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound(new { success = false, message = "Book not found." });

        existing.Title = request.Title.Trim();
        existing.Author = request.Author.Trim();
        existing.Description = request.Description.Trim();
        existing.Price = Math.Round(request.Price, 2, MidpointRounding.AwayFromZero);
        existing.Genre = request.Genre.Trim().ToLowerInvariant();
        existing.Isbn = string.IsNullOrWhiteSpace(request.Isbn) ? null : request.Isbn.Trim();
        existing.PublishedOn = request.PublishedOn;
        existing.IsAvailable = request.IsAvailable;

        if (request.CoverImage != null)
            try
            {
                var (coverBase64, coverMime) = await ReadCoverAsync(request.CoverImage, cancellationToken);
                if (!string.IsNullOrWhiteSpace(coverBase64))
                {
                    existing.CoverImageBase64 = coverBase64;
                    existing.CoverImageMimeType = coverMime;
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cover validation failed while updating book {BookId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        if (updated == null)
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { success = false, message = "Unable to update book." });

        var response = updated.ToResponse();
        return Ok(new { success = true, message = "Book updated successfully.", item = response });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBook(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { success = false, message = "Book not found." });

        return Ok(new { success = true, message = "Book deleted successfully." });
    }

    private async Task<(string? Base64, string? MimeType)> ReadCoverAsync(IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return (null, null);

        if (file.Length > MaxImageSizeBytes)
            throw new InvalidOperationException($"Cover image exceeds {MaxImageSizeBytes / 1024 / 1024} MB limit.");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var base64 = Convert.ToBase64String(memoryStream.ToArray());
        return (base64, file.ContentType);
    }

    private static bool ShouldSortDescending(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return false;

        return sort.StartsWith("-");
    }

    private static BookSortField ParseSortField(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return BookSortField.CreatedAt;

        var value = sort.TrimStart('-', '+').ToLowerInvariant();

        return value switch
        {
            "title" => BookSortField.Title,
            "author" => BookSortField.Author,
            "price" => BookSortField.Price,
            "publishedon" or "published" => BookSortField.PublishedOn,
            "updated" or "updatedat" => BookSortField.UpdatedAt,
            "created" or "createdat" => BookSortField.CreatedAt,
            _ => BookSortField.CreatedAt
        };
    }
}