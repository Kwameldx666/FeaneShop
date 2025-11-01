using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Tests.Controllers;

public class ProductsControllerTests : IDisposable
{
    private readonly ProductDbContext _context;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ProductDbContext(options);
        _controller = new ProductsController(_context);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task GetProducts_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Dish>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Pizza Margherita",
                Price = 15.00m,
                Category = "Pizza",
                IsAvailable = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Burger Deluxe",
                Price = 12.50m,
                Category = "Burgers",
                IsAvailable = true
            }
        };

        _context.Dishes.AddRange(products);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetProducts();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Dish
        {
            Id = productId,
            Name = "Caesar Salad",
            Price = 10.00m,
            Category = "Salads",
            IsAvailable = true
        };

        _context.Dishes.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Caesar Salad");
        result.Value.Price.Should().Be(10.00m);
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.GetProduct(invalidId);

        // Assert
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task CreateProduct_WithValidData_CreatesProduct()
    {
        // Arrange
        var newProduct = new Dish
        {
            Name = "Spaghetti Carbonara",
            Price = 14.00m,
            Category = "Pasta",
            Description = "Classic Italian pasta",
            IsAvailable = true
        };

        // Act
        var result = await _controller.CreateProduct(newProduct);

        // Assert
        result.Should().NotBeNull();
        var createdProduct = await _context.Dishes
            .FirstOrDefaultAsync(d => d.Name == "Spaghetti Carbonara");

        createdProduct.Should().NotBeNull();
        createdProduct!.Price.Should().Be(14.00m);
        createdProduct.Category.Should().Be("Pasta");
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_UpdatesProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Dish
        {
            Id = productId,
            Name = "Old Name",
            Price = 10.00m,
            Category = "Category",
            IsAvailable = true
        };

        _context.Dishes.Add(product);
        await _context.SaveChangesAsync();

        product.Name = "New Name";
        product.Price = 15.00m;

        // Act
        var result = await _controller.UpdateProduct(productId, product);

        // Assert
        result.Should().NotBeNull();
        var updatedProduct = await _context.Dishes.FindAsync(productId);
        updatedProduct!.Name.Should().Be("New Name");
        updatedProduct.Price.Should().Be(15.00m);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_DeletesProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Dish
        {
            Id = productId,
            Name = "To Delete",
            Price = 5.00m,
            Category = "Test",
            IsAvailable = true
        };

        _context.Dishes.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        result.Should().NotBeNull();
        var deletedProduct = await _context.Dishes.FindAsync(productId);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task GetProductsByCategory_ReturnsFilteredProducts()
    {
        // Arrange
        var products = new List<Dish>
        {
            new() { Id = Guid.NewGuid(), Name = "Pizza 1", Price = 15.00m, Category = "Pizza", IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "Pizza 2", Price = 16.00m, Category = "Pizza", IsAvailable = true },
            new() { Id = Guid.NewGuid(), Name = "Burger 1", Price = 12.00m, Category = "Burgers", IsAvailable = true }
        };

        _context.Dishes.AddRange(products);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetProductsByCategory("Pizza");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Category == "Pizza");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetAvailableProducts_ReturnsCorrectProducts(bool isAvailable)
    {
        // Arrange
        var products = new List<Dish>
        {
            new() { Id = Guid.NewGuid(), Name = "Available", Price = 10.00m, Category = "Test", IsAvailable = true },
            new()
            {
                Id = Guid.NewGuid(), Name = "Not Available", Price = 10.00m, Category = "Test", IsAvailable = false
            }
        };

        _context.Dishes.AddRange(products);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAvailableProducts(isAvailable);

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(p => p.IsAvailable == isAvailable);
    }
}