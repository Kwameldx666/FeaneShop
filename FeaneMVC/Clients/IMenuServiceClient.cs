using Feane.Contracts.Dishes;

namespace FeaneMVC.Clients;

public interface IMenuServiceClient
{
    Task<IReadOnlyCollection<DishResponse>> GetDishesAsync(CancellationToken cancellationToken = default);
    Task<DishResponse?> GetDishAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DishResponse?> CreateDishAsync(CreateDishRequest request, CancellationToken cancellationToken = default);
    Task<DishResponse?> UpdateDishAsync(Guid id, UpdateDishRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteDishAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(int created, int skipped)> SeedAsync(int count, CancellationToken cancellationToken = default);
}
