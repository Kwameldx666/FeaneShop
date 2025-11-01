using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookService.Application.Interfaces;
using BookService.Application.DTOs;
using BookService.Domain.Entities;
using BookService.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using BookService.Infrastructure.Persistence;
using Xunit;

namespace BookService.Tests.Repositories;

public class BookRepositoryTests : IDisposable
{
    private readonly BookDbContext _context;
    private readonly IBookRepository _repository;
    private readonly Mock<ILogger<BookRepository>> _mockLogger;

    public BookRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BookDbContext(options);
        _mockLogger = new Mock<ILogger<BookRepository>>();
        _repository = new BookRepository(_context, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task AddAsync_AddsBookToDatabase()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test Book",
            Author = "Test Author",
            Description = "Test Description",
            Price = 19.99m,
            Genre = "fiction",
            IsAvailable = true
        };

        // Act
        var result = await _repository.AddAsync(book, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        var savedBook = await _context.Books.FindAsync(result.Id);
        savedBook.Should().NotBeNull();
        savedBook!.Title.Should().Be("Test Book");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsBook_WhenExists()
    {
        // Arrange
        var book = new Book
        {
            Title = "Existing Book",
            Author = "Author",
            Price = 25.00m,
            Genre = "mystery"
        };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(book.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Existing Book");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesBookInDatabase()
    {
        // Arrange
        var book = new Book
        {
            Title = "Original Title",
            Author = "Author",
            Price = 20.00m,
            Genre = "fiction"
        };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Act
        book.Title = "Updated Title";
        book.Price = 25.00m;
        await _repository.UpdateAsync(book, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var updated = await _context.Books.FindAsync(book.Id);
        updated!.Title.Should().Be("Updated Title");
        updated.Price.Should().Be(25.00m);
    }

    [Fact]
    public async Task DeleteAsync_RemovesBookFromDatabase()
    {
        // Arrange
        var book = new Book
        {
            Title = "To Delete",
            Author = "Author",
            Price = 15.00m,
            Genre = "thriller"
        };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(book.Id, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Books.FindAsync(book.Id);
        deleted.Should().BeNull();
    }

    [Theory]
    [InlineData("fiction")]
    [InlineData("science")]
    [InlineData("history")]
    public async Task GetAsync_FiltersByGenre(string genre)
    {
        // Arrange
        var books = new List<Book>
        {
            new() { Title = "Fiction Book", Author = "A1", Price = 10m, Genre = "fiction" },
            new() { Title = "Science Book", Author = "A2", Price = 15m, Genre = "science" },
            new() { Title = "History Book", Author = "A3", Price = 20m, Genre = "history" }
        };
        _context.Books.AddRange(books);
        await _context.SaveChangesAsync();

        // Act
        var options = new BookQueryOptions { Genre = genre };
        var result = await _repository.GetAsync(options, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Genre.Should().Be(genre);
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var books = new List<Book>
        {
            new() { Title = "Book 1", Author = "A1", Price = 10m, Genre = "fiction" },
            new() { Title = "Book 2", Author = "A2", Price = 15m, Genre = "fiction" },
            new() { Title = "Book 3", Author = "A3", Price = 20m, Genre = "science" }
        };
        _context.Books.AddRange(books);
        await _context.SaveChangesAsync();

        // Act
        var options = new BookQueryOptions { Genre = "fiction" };
        var count = await _repository.CountAsync(options, CancellationToken.None);

        // Assert
        count.Should().Be(2);
    }
}

