using CartService.Application.Interfaces;
using CartService.Domain.Entities;
using CartService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CartService.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly CartDbContext _context;
    private readonly ILogger<CartRepository> _logger;

    public CartRepository(CartDbContext context, ILogger<CartRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<CartItem>> GetItemsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.CartItems
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.UpdatedAt)
            .ThenByDescending(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<CartItem?> GetItemAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken = default)
    {
        return _context.CartItems
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == cartItemId && item.UserId == userId, cancellationToken);
    }

    public async Task<CartItem> AddOrUpdateAsync(CartItem item, CancellationToken cancellationToken = default)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        try
        {
            var existing = await _context.CartItems
                .FirstOrDefaultAsync(i => i.UserId == item.UserId && i.ProductId == item.ProductId, cancellationToken);

            if (existing is null)
            {
                item.Id = Guid.NewGuid();
                item.CreatedAt = DateTime.UtcNow;
                item.UpdatedAt = item.CreatedAt;
                item.Quantity = Math.Clamp(item.Quantity, 1, 100);
                _context.CartItems.Add(item);
                await _context.SaveChangesAsync(cancellationToken);
                return item;
            }

            existing.Quantity = Math.Clamp(existing.Quantity + item.Quantity, 1, 100);
            existing.UnitPrice = item.UnitPrice;
            existing.ProductName = item.ProductName;
            existing.ProductImageUrl = item.ProductImageUrl;
            existing.Notes = item.Notes;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add/update cart item for user {UserId} and product {ProductId}",
                item.UserId, item.ProductId);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Guid userId, Guid cartItemId, Action<CartItem> updateAction,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.CartItems.FirstOrDefaultAsync(i => i.Id == cartItemId && i.UserId == userId,
            cancellationToken);
        if (item is null) return false;

        updateAction(item);
        item.Quantity = Math.Clamp(item.Quantity, 1, 100);
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var item = await _context.CartItems.FirstOrDefaultAsync(i => i.Id == cartItemId && i.UserId == userId,
            cancellationToken);
        if (item is null) return false;

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> ClearAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await _context.CartItems.Where(i => i.UserId == userId).ToListAsync(cancellationToken);
        if (items.Count == 0) return 0;

        _context.CartItems.RemoveRange(items);
        return await _context.SaveChangesAsync(cancellationToken);
    }
}