using BookService.Application.DTOs;
using BookService.Domain.Entities;

namespace BookService.Application.Interfaces;

public interface IBookRepository
{
    Task<IReadOnlyList<Book>> GetAsync(BookQueryOptions options, CancellationToken cancellationToken = default);
    Task<int> CountAsync(BookQueryOptions options, CancellationToken cancellationToken = default);
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default);
    Task<Book?> UpdateAsync(Book book, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken cancellationToken = default);
}