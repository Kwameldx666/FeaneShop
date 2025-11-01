using BookService.Application.DTOs;
using BookService.Application.Interfaces;
using BookService.Domain.Entities;
using BookService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookService.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly BookDbContext _context;
    private readonly ILogger<BookRepository> _logger;

    public BookRepository(BookDbContext context, ILogger<BookRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<Book>> GetAsync(BookQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = BuildQuery(options);

            if (options.Limit.HasValue && options.Limit > 0)
            {
                query = query.Take(options.Limit.Value);
            }
            else if (options.Page.HasValue && options.PageSize.HasValue && options.Page.Value >= 1 &&
                     options.PageSize.Value > 0)
            {
                var skip = (options.Page.Value - 1) * options.PageSize.Value;
                query = query.Skip(skip).Take(options.PageSize.Value);
            }

            return await query.AsNoTracking().ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load books with options {@Options}", options);
            return Array.Empty<Book>();
        }
    }

    public async Task<int> CountAsync(BookQueryOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = BuildQuery(options, false);
            return await query.CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count books with options {@Options}", options);
            return 0;
        }
    }

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        book.Id = book.Id == Guid.Empty ? Guid.NewGuid() : book.Id;
        book.CreatedAt = DateTime.UtcNow;
        book.UpdatedAt = DateTime.UtcNow;

        _context.Books.Add(book);
        await _context.SaveChangesAsync(cancellationToken);
        return book;
    }

    public async Task<Book?> UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Books.FirstOrDefaultAsync(b => b.Id == book.Id, cancellationToken);
        if (existing == null) return null;

        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.Description = book.Description;
        existing.Price = book.Price;
        existing.Genre = book.Genre;
        existing.Isbn = book.Isbn;
        existing.PublishedOn = book.PublishedOn;
        existing.IsAvailable = book.IsAvailable;
        if (!string.IsNullOrWhiteSpace(book.CoverImageBase64))
        {
            existing.CoverImageBase64 = book.CoverImageBase64;
            existing.CoverImageMimeType = book.CoverImageMimeType;
        }

        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (entity == null) return false;

        _context.Books.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Books
            .AsNoTracking()
            .Where(b => b.IsAvailable)
            .Select(b => b.Genre)
            .Distinct()
            .OrderBy(genre => genre)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Book> BuildQuery(BookQueryOptions? options, bool applyOrdering = true)
    {
        options ??= new BookQueryOptions();

        var query = _context.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(options.Genre))
        {
            var genre = options.Genre.Trim().ToLowerInvariant();
            query = query.Where(b => b.Genre.ToLower() == genre);
        }

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            var term = options.Search.Trim().ToLowerInvariant();
            query = query.Where(b =>
                b.Title.ToLower().Contains(term) ||
                b.Author.ToLower().Contains(term) ||
                b.Description.ToLower().Contains(term) ||
                (b.Isbn != null && b.Isbn.ToLower().Contains(term)));
        }

        if (options.AvailableOnly) query = query.Where(b => b.IsAvailable);

        if (!applyOrdering) return query;

        query = options.SortBy switch
        {
            BookSortField.Title => options.Descending
                ? query.OrderByDescending(b => b.Title)
                : query.OrderBy(b => b.Title),
            BookSortField.Author => options.Descending
                ? query.OrderByDescending(b => b.Author)
                : query.OrderBy(b => b.Author),
            BookSortField.Price => options.Descending
                ? query.OrderByDescending(b => b.Price)
                : query.OrderBy(b => b.Price),
            BookSortField.PublishedOn => options.Descending
                ? query.OrderByDescending(b => b.PublishedOn)
                : query.OrderBy(b => b.PublishedOn),
            BookSortField.UpdatedAt => options.Descending
                ? query.OrderByDescending(b => b.UpdatedAt)
                : query.OrderBy(b => b.UpdatedAt),
            BookSortField.CreatedAt => options.Descending
                ? query.OrderByDescending(b => b.CreatedAt)
                : query.OrderBy(b => b.CreatedAt),
            _ => query.OrderByDescending(b => b.CreatedAt)
        };

        return query;
    }
}