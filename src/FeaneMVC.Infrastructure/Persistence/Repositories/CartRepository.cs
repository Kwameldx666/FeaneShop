using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Infrastructure.Persistence.Db;
using Microsoft.EntityFrameworkCore;

namespace FeaneMVC.Infrastructure.Persistence.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CartRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Cart?> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Cart
            .Include(cart => cart.CartItems)
            .FirstOrDefaultAsync(cart => cart.UserId == userId, cancellationToken);
    }

    public async Task<Cart> GetOrCreateCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(userId, cancellationToken);
        if (cart != null)
        {
            return cart;
        }

        cart = new Cart
        {
            CartId = Guid.NewGuid(),
            UserId = userId,
            CartItems = new List<CartItem>()
        };

        await _dbContext.Cart.AddAsync(cart, cancellationToken);

        return cart;
    }
}
