using FeaneMVC.Domain.Entities;

namespace FeaneMVC.Application.Common.Interfaces.Services;

public interface ICartService
{
    Task AddItemToCartAsync(Guid userId, CartItem item);

    Task RemoveItemFromCartAsync(Guid userId, Guid dishId);

    Task UpdateItemQuantityAsync(Guid userId, Guid dishId, int quantity);

    Task<Cart> GetCartAsync(Guid userId);

    Task<decimal> CalculateTotalAsync(Guid userId);
}
