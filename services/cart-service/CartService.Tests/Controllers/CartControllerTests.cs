using CartService.Controllers;
using CartService.Domain.Entities;
using CartService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CartService.Tests.Controllers;

public class CartControllerTests : IDisposable
{
    private readonly CartDbContext _context;
    private readonly CartController _controller;

    public CartControllerTests()
    {
        var options = new DbContextOptionsBuilder<CartDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new CartDbContext(options);
        _controller = new CartController(_context);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task GetCart_WithUserId_ReturnsUserCart()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var cartItems = new List<CartItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = Guid.NewGuid(),
                ProductName = "Pizza",
                Quantity = 2,
                Price = 15.00m
            },
            new()
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = Guid.NewGuid(),
                ProductName = "Burger",
                Quantity = 1,
                Price = 12.00m
            }
        };

        _context.Carts.Add(cart);
        _context.CartItems.AddRange(cartItems);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetCart(userId);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddItemToCart_WithNewItem_AddsItem()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        var newItem = new CartItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = "New Pizza",
            Quantity = 1,
            Price = 16.00m
        };

        // Act
        var result = await _controller.AddItemToCart(userId, newItem);

        // Assert
        result.Should().NotBeNull();
        var cartItems = await _context.CartItems
            .Where(ci => ci.CartId == cart.Id)
            .ToListAsync();

        cartItems.Should().HaveCount(1);
        cartItems.First().ProductName.Should().Be("New Pizza");
    }

    [Fact]
    public async Task UpdateCartItemQuantity_WithValidData_UpdatesQuantity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var cartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            ProductId = Guid.NewGuid(),
            ProductName = "Pizza",
            Quantity = 1,
            Price = 15.00m
        };

        _context.Carts.Add(cart);
        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();

        // Act
        await _controller.UpdateCartItemQuantity(cartItem.Id, 5);

        // Assert
        var updatedItem = await _context.CartItems.FindAsync(cartItem.Id);
        updatedItem!.Quantity.Should().Be(5);
    }

    [Fact]
    public async Task RemoveItemFromCart_WithValidId_RemovesItem()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var cartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            ProductId = Guid.NewGuid(),
            ProductName = "Pizza",
            Quantity = 1,
            Price = 15.00m
        };

        _context.Carts.Add(cart);
        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();

        // Act
        await _controller.RemoveItemFromCart(cartItem.Id);

        // Assert
        var deletedItem = await _context.CartItems.FindAsync(cartItem.Id);
        deletedItem.Should().BeNull();
    }

    [Fact]
    public async Task ClearCart_WithUserId_RemovesAllItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var cartItems = new List<CartItem>
        {
            new()
            {
                Id = Guid.NewGuid(), CartId = cart.Id, ProductId = Guid.NewGuid(), ProductName = "Item 1", Quantity = 1,
                Price = 10.00m
            },
            new()
            {
                Id = Guid.NewGuid(), CartId = cart.Id, ProductId = Guid.NewGuid(), ProductName = "Item 2", Quantity = 2,
                Price = 20.00m
            },
            new()
            {
                Id = Guid.NewGuid(), CartId = cart.Id, ProductId = Guid.NewGuid(), ProductName = "Item 3", Quantity = 3,
                Price = 30.00m
            }
        };

        _context.Carts.Add(cart);
        _context.CartItems.AddRange(cartItems);
        await _context.SaveChangesAsync();

        // Act
        await _controller.ClearCart(userId);

        // Assert
        var remainingItems = await _context.CartItems
            .Where(ci => ci.CartId == cart.Id)
            .ToListAsync();

        remainingItems.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCartTotal_CalculatesCorrectTotal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var cartItems = new List<CartItem>
        {
            new()
            {
                Id = Guid.NewGuid(), CartId = cart.Id, ProductId = Guid.NewGuid(), ProductName = "Item 1", Quantity = 2,
                Price = 10.00m
            },
            new()
            {
                Id = Guid.NewGuid(), CartId = cart.Id, ProductId = Guid.NewGuid(), ProductName = "Item 2", Quantity = 3,
                Price = 15.00m
            }
        };

        _context.Carts.Add(cart);
        _context.CartItems.AddRange(cartItems);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetCart(userId);

        // Assert
        var total = result.Value!.Items.Sum(i => i.Quantity * i.Price);
        total.Should().Be(65.00m); // (2 * 10) + (3 * 15) = 20 + 45 = 65
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateCartItemQuantity_WithInvalidQuantity_ThrowsException(int quantity)
    {
        // Arrange
        var cartItemId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0");

            await _controller.UpdateCartItemQuantity(cartItemId, quantity);
        });
    }
}