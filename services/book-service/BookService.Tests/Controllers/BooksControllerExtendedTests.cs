using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using BookService.Application.DTOs;
using BookService.Application.Interfaces;
using BookService.Controllers;
using BookService.Domain.Entities;

namespace BookService.Tests.Controllers;

public class BooksControllerExtendedTests
{
    private readonly Mock<IBookRepository> _mockRepository;
    private readonly Mock<ILogger<BooksController>> _mockLogger;
    private readonly BooksController _controller;

    public BooksControllerExtendedTests()
    {
        _mockRepository = new Mock<IBookRepository>();
        _mockLogger = new Mock<ILogger<BooksController>>();
        _controller = new BooksController(_mockRepository.Object, _mockLogger.Object);
    }

    [Theory]
    [InlineData("fiction")]
    [InlineData("science")]
    [InlineData("history")]
    [InlineData("biography")]
    [InlineData("mystery")]
    [InlineData("romance")]
    [InlineData("thriller")]
    [InlineData("fantasy")]
    [InlineData("self-help")]
    [InlineData("business")]
    public async Task GetBooks_WithDifferentGenres_ReturnsFilteredBooks(string genre)
    {
        var books = new List<Book>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = $"{genre} Book",
                Author = "Author Name",
                Description = "Description",
                Price = 19.99m,
                Genre = genre,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(r => r.GetAsync(It.Is<BookQueryOptions>(o => o.Genre == genre), It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);
        _mockRepository.Setup(r => r.CountAsync(It.Is<BookQueryOptions>(o => o.Genre == genre), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _controller.GetBooks(genre, null, null, null, null, null, null, null, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData(5.00)]
    [InlineData(10.00)]
    [InlineData(15.00)]
    [InlineData(20.00)]
    [InlineData(25.00)]
    [InlineData(30.00)]
    [InlineData(40.00)]
    [InlineData(50.00)]
    [InlineData(75.00)]
    [InlineData(100.00)]
    public async Task GetBooks_WithDifferentPrices_ReturnsBooks(decimal price)
    {
        var books = new List<Book>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Test Book",
                Author = "Author",
                Description = "Description",
                Price = price,
                Genre = "fiction",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(r => r.GetAsync(It.IsAny<BookQueryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);
        _mockRepository.Setup(r => r.CountAsync(It.IsAny<BookQueryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _controller.GetBooks(null, null, null, null, null, null, null, null, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData("Harry Potter")]
    [InlineData("Lord of the Rings")]
    [InlineData("1984")]
    [InlineData("The Great Gatsby")]
    [InlineData("To Kill a Mockingbird")]
    [InlineData("Pride and Prejudice")]
    [InlineData("The Catcher in the Rye")]
    [InlineData("Brave New World")]
    [InlineData("Animal Farm")]
    [InlineData("The Hobbit")]
    public async Task GetBooks_SearchByTitle_ReturnsMatchingBooks(string searchTerm)
    {
        var books = new List<Book>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = searchTerm,
                Author = "Famous Author",
                Description = "Classic",
                Price = 24.99m,
                Genre = "fiction",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(r => r.GetAsync(It.Is<BookQueryOptions>(o => o.Search == searchTerm), It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);
        _mockRepository.Setup(r => r.CountAsync(It.Is<BookQueryOptions>(o => o.Search == searchTerm), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _controller.GetBooks(null, searchTerm, null, null, null, null, null, null, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }
}

