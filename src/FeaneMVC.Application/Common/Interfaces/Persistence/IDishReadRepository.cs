using FeaneMVC.Domain.Entities;

namespace FeaneMVC.Application.Common.Interfaces.Persistence;

public interface IDishReadRepository
{
    Task<IReadOnlyList<Dish>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Dish?> GetByIdAsync(Guid dishId, CancellationToken cancellationToken = default);
}
