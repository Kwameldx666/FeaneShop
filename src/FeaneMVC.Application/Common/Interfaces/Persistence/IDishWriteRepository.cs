using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;

namespace FeaneMVC.Application.Common.Interfaces.Persistence;

public interface IDishWriteRepository
{
    Task<OperationResult<Dish>> AddAsync(Dish dish, CancellationToken cancellationToken = default);
    Task<OperationResult<Dish>> UpdateAsync(Guid dishId, Dish dish, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(Guid dishId, CancellationToken cancellationToken = default);
    Task<OperationResult<BulkSeedSummary>> SeedRandomAsync(int count, CancellationToken cancellationToken = default);
}
