using Feane.Contracts.Dishes;
using MenuService.Models;

namespace MenuService.Infrastructure;

public interface IDishRepository
{
    Task<IReadOnlyCollection<DishDocument>> GetAllAsync(CancellationToken cancellationToken);
    Task<DishDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<DishDocument> CreateAsync(CreateDishRequest request, CancellationToken cancellationToken);
    Task<DishDocument?> UpdateAsync(Guid id, UpdateDishRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<(int created, int skipped)> SeedAsync(int count, CancellationToken cancellationToken);
}
