using CartService.Domain.Entities;

namespace CartService.Application.Interfaces;

public interface ICartRepository
{
    Task<IReadOnlyList<CartItem>> GetItemsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CartItem?> GetItemAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken = default);
    Task<CartItem> AddOrUpdateAsync(CartItem item, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Guid userId, Guid cartItemId, Action<CartItem> updateAction,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken = default);
    Task<int> ClearAsync(Guid userId, CancellationToken cancellationToken = default);
}