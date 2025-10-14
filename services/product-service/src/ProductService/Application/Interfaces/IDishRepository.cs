using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface IDishRepository
{
    Task<IReadOnlyList<Dish>> GetAsync(DishQueryOptions options, CancellationToken cancellationToken = default);
    Task<int> CountAsync(DishQueryOptions options, CancellationToken cancellationToken = default);
    Task<Dish?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Dish> AddAsync(Dish dish, CancellationToken cancellationToken = default);
    Task<Dish?> UpdateAsync(Dish dish, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}
