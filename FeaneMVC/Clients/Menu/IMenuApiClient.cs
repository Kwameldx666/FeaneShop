using Feane.Contracts.Dishes;

namespace FeaneMVC.Clients.Menu;

public interface IMenuApiClient
{
    Task<IReadOnlyList<DishResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DishResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DishResponse?> CreateAsync(CreateDishRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UpdateDishRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
