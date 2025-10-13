using Microsoft.AspNetCore.Mvc;
using OrderService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IOrderStore, InMemoryOrderStore>();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "order-service", status = "healthy" }));

app.Run();

namespace OrderService.Models;

public interface IOrderStore
{
    Order Create(CreateOrderRequest request);
    Order? GetById(Guid id);
    IEnumerable<Order> GetForUser(Guid userId);
    Order UpdateStatus(Guid id, OrderStatus status);
}

public sealed class InMemoryOrderStore : IOrderStore
{
    private readonly Dictionary<Guid, Order> _orders = new();

    public Order Create(CreateOrderRequest request)
    {
        var order = new Order(Guid.NewGuid(), request.UserId, DateTimeOffset.UtcNow, OrderStatus.Pending, request.Items);
        _orders[order.Id] = order;
        return order;
    }

    public Order? GetById(Guid id) => _orders.TryGetValue(id, out var order) ? order : null;

    public IEnumerable<Order> GetForUser(Guid userId) => _orders.Values.Where(o => o.UserId == userId);

    public Order UpdateStatus(Guid id, OrderStatus status)
    {
        if (!_orders.TryGetValue(id, out var order))
        {
            throw new KeyNotFoundException($"Order {id} not found");
        }

        var updated = order with { Status = status };
        _orders[id] = updated;
        return updated;
    }
}

public record Order(Guid Id, Guid UserId, DateTimeOffset CreatedAt, OrderStatus Status, IReadOnlyCollection<OrderLine> Items);

public record OrderLine(Guid ProductId, int Quantity, decimal Price);

public record CreateOrderRequest(Guid UserId, IReadOnlyCollection<OrderLine> Items);

public enum OrderStatus
{
    Pending,
    Paid,
    Fulfilled,
    Cancelled
}
