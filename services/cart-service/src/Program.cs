using Microsoft.AspNetCore.Mvc;
using CartService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<ICartStore, InMemoryCartStore>();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "cart-service", status = "healthy" }));

app.Run();

namespace CartService.Models;

public interface ICartStore
{
    Cart GetCart(Guid userId);
    CartItem AddItem(Guid userId, AddCartItemRequest request);
    void RemoveItem(Guid userId, Guid itemId);
}

public sealed class InMemoryCartStore : ICartStore
{
    private readonly Dictionary<Guid, Cart> _carts = new();

    public Cart GetCart(Guid userId)
    {
        if (!_carts.TryGetValue(userId, out var cart))
        {
            cart = new Cart(userId, []);
            _carts[userId] = cart;
        }

        return cart with { Items = cart.Items.ToList() };
    }

    public CartItem AddItem(Guid userId, AddCartItemRequest request)
    {
        var cart = GetCart(userId);
        var newItem = new CartItem(Guid.NewGuid(), request.ProductId, request.Quantity);
        var items = cart.Items.ToList();
        items.Add(newItem);
        _carts[userId] = cart with { Items = items };
        return newItem;
    }

    public void RemoveItem(Guid userId, Guid itemId)
    {
        var cart = GetCart(userId);
        var filtered = cart.Items.Where(i => i.Id != itemId).ToList();
        _carts[userId] = cart with { Items = filtered };
    }
}

public record Cart(Guid UserId, IReadOnlyCollection<CartItem> Items);

public record CartItem(Guid Id, Guid ProductId, int Quantity);

public record AddCartItemRequest(Guid ProductId, int Quantity);
