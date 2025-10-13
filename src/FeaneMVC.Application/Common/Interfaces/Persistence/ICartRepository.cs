using FeaneMVC.Domain.Entities;

namespace FeaneMVC.Application.Common.Interfaces.Persistence;

public interface ICartRepository
{
    Task<Cart?> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Cart> GetOrCreateCartAsync(Guid userId, CancellationToken cancellationToken = default);
}
