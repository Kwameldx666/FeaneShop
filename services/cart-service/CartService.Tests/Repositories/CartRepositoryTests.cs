using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CartService.Application.Interfaces;
using CartService.Domain.Entities;
using CartService.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using CartService.Infrastructure.Persistence;
using Xunit;

namespace CartService.Tests.Repositories;

public class CartRepositoryTests : IDisposable
{
    private readonly CartDbContext _context;
    private readonly ICartRepository _repository;
    private readonly Mock<ILogger<CartRepository>> _mockLogger;
    private readonly Guid _testUserId = Guid.NewGuid();

    public CartRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CartDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new CartDbContext(options);
        _mockLogger = new Mock<ILogger<CartRepository>>();
        _repository = new CartRepository(_context, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task AddOrUpdateAsync_AddsNewItem()
    {
        // Arrange
        var item = new CartItem
        {
            UserId = _testUserId,
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            Quantity = 2,
            UnitPrice = 10.00m
        };

        // Act
        var result = await _repository.AddOrUpdateAsync(item, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        var saved = await _context.CartItems.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsUserItems()
    {
        // Arrange
        var items = new List<CartItem>
        {
            new() { UserId = _testUserId, ProductId = Guid.NewGuid(), ProductName = "P1", Quantity = 1, UnitPrice = 10m },
            new() { UserId = _testUserId, ProductId = Guid.NewGuid(), ProductName = "P2", Quantity = 2, UnitPrice = 15m },
            new() { UserId = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "P3", Quantity = 1, UnitPrice = 20m }
        };
        _context.CartItems.AddRange(items);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetItemsAsync(_testUserId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(i => i.UserId == _testUserId);
    }

    [Fact]
    public async Task RemoveAsync_RemovesItem()
    {
        // Arrange
        var item = new CartItem
        {
            UserId = _testUserId,
            ProductId = Guid.NewGuid(),
            ProductName = "To Remove",
            Quantity = 1,
            UnitPrice = 10m
        };
        _context.CartItems.Add(item);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.RemoveAsync(_testUserId, item.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var removed = await _context.CartItems.FindAsync(item.Id);
        removed.Should().BeNull();
    }

    [Fact]
    public async Task ClearAsync_RemovesAllUserItems()
    {
        // Arrange
        var items = new List<CartItem>
        {
            new() { UserId = _testUserId, ProductId = Guid.NewGuid(), ProductName = "P1", Quantity = 1, UnitPrice = 10m },
            new() { UserId = _testUserId, ProductId = Guid.NewGuid(), ProductName = "P2", Quantity = 2, UnitPrice = 15m }
        };
        _context.CartItems.AddRange(items);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.ClearAsync(_testUserId, CancellationToken.None);

        // Assert
        count.Should().Be(2);
        var remaining = await _context.CartItems.Where(i => i.UserId == _testUserId).ToListAsync();
        remaining.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task UpdateAsync_UpdatesQuantity(int newQuantity)
    {
        // Arrange
        var item = new CartItem
        {
            UserId = _testUserId,
            ProductId = Guid.NewGuid(),
            ProductName = "Product",
            Quantity = 1,
            UnitPrice = 10m
        };
        _context.CartItems.Add(item);
        await _context.SaveChangesAsync();

        // Act
        var success = await _repository.UpdateAsync(_testUserId, item.Id, i => i.Quantity = newQuantity, CancellationToken.None);

        // Assert
        success.Should().BeTrue();
        var updated = await _context.CartItems.FindAsync(item.Id);
        updated!.Quantity.Should().Be(newQuantity);
    }
}

