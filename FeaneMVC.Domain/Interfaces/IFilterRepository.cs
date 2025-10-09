using FeaneMVC.Domain.Entities;

namespace FeaneMVC.Domain.Interfaces;

public interface IFilterRepository
{
    Task<IReadOnlyList<Filter>> GetUserFiltersAsync(Guid userId);

    Task AddFiltersAsync(Guid userId, IReadOnlyCollection<string> filters);
}
