using Microsoft.AspNetCore.Mvc;
using ProductService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IProductCatalog, InMemoryProductCatalog>();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "product-service", status = "healthy" }));

app.Run();

namespace ProductService.Models;

public interface IProductCatalog
{
    IEnumerable<Product> GetAll();
    Product? GetById(Guid id);
    Product Create(CreateProductRequest request);
}

public sealed class InMemoryProductCatalog : IProductCatalog
{
    private readonly Dictionary<Guid, Product> _products = new();

    public InMemoryProductCatalog()
    {
        var starter = new Product(Guid.NewGuid(), "Margherita Pizza", "Classic tomato and mozzarella pizza", 12.50m);
        _products[starter.Id] = starter;
    }

    public IEnumerable<Product> GetAll() => _products.Values;

    public Product? GetById(Guid id) => _products.TryGetValue(id, out var product) ? product : null;

    public Product Create(CreateProductRequest request)
    {
        var product = new Product(Guid.NewGuid(), request.Name, request.Description, request.Price);
        _products[product.Id] = product;
        return product;
    }
}

public record Product(Guid Id, string Name, string? Description, decimal Price);

public record CreateProductRequest(string Name, string? Description, decimal Price);
