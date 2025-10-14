using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Infrastructure.Repositories;

public class DishRepository : IDishRepository
{
    private readonly ProductDbContext _context;
    private readonly ILogger<DishRepository> _logger;

    public DishRepository(ProductDbContext context, ILogger<DishRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<Dish>> GetAsync(DishQueryOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = BuildQuery(options);

            if (options.Limit.HasValue && options.Limit > 0)
            {
                query = query.Take(options.Limit.Value);
            }
            else if (options.Page.HasValue && options.PageSize.HasValue && options.Page.Value >= 1 && options.PageSize.Value > 0)
            {
                var skip = (options.Page.Value - 1) * options.PageSize.Value;
                query = query.Skip(skip).Take(options.PageSize.Value);
            }

            return await query.AsNoTracking().ToListAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve dishes with options {@Options}", options);
            return Array.Empty<Dish>();
        }
    }

    public async Task<int> CountAsync(DishQueryOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = BuildQuery(options, applyOrdering: false);
            return await query.CountAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to count dishes with options {@Options}", options);
            return 0;
        }
    }

    public Task<Dish?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Dishes.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Dish> AddAsync(Dish dish, CancellationToken cancellationToken = default)
    {
        dish.Id = dish.Id == Guid.Empty ? Guid.NewGuid() : dish.Id;
        dish.CreatedAt = DateTime.UtcNow;
        dish.UpdatedAt = DateTime.UtcNow;

        _context.Dishes.Add(dish);
        await _context.SaveChangesAsync(cancellationToken);
        return dish;
    }

    public async Task<Dish?> UpdateAsync(Dish dish, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Dishes.FirstOrDefaultAsync(d => d.Id == dish.Id, cancellationToken);
        if (existing == null)
        {
            return null;
        }

        existing.Name = dish.Name;
        existing.Description = dish.Description;
        existing.Price = dish.Price;
        existing.Category = dish.Category;
        existing.IsAvailable = dish.IsAvailable;
        existing.IsFeatured = dish.IsFeatured;
        existing.PopularityScore = dish.PopularityScore;
        existing.ImageBase64 = dish.ImageBase64;
        existing.ImageMimeType = dish.ImageMimeType;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dish = await _context.Dishes.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (dish == null)
        {
            return false;
        }

        _context.Dishes.Remove(dish);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Dishes
            .AsNoTracking()
            .Where(d => d.IsAvailable)
            .Select(d => d.Category)
            .Distinct()
            .OrderBy(category => category)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Dish> BuildQuery(DishQueryOptions options, bool applyOrdering = true)
    {
        options ??= new DishQueryOptions();
        var query = _context.Dishes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(options.Category))
        {
            var category = options.Category.Trim().ToLowerInvariant();
            query = query.Where(d => d.Category.ToLower() == category);
        }

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            var term = options.Search.Trim().ToLowerInvariant();
            query = query.Where(d => d.Name.ToLower().Contains(term) ||
                                     d.Description.ToLower().Contains(term) ||
                                     d.Category.ToLower().Contains(term));
        }

        if (options.AvailableOnly)
        {
            query = query.Where(d => d.IsAvailable);
        }

        if (!applyOrdering)
        {
            return query;
        }

        query = options.SortBy switch
        {
            DishSortField.Name => options.Descending ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
            DishSortField.Price => options.Descending ? query.OrderByDescending(d => d.Price) : query.OrderBy(d => d.Price),
            DishSortField.UpdatedAt => options.Descending ? query.OrderByDescending(d => d.UpdatedAt) : query.OrderBy(d => d.UpdatedAt),
            DishSortField.Popularity => options.Descending ? query.OrderByDescending(d => d.PopularityScore) : query.OrderByDescending(d => d.PopularityScore),
            DishSortField.CreatedAt => options.Descending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };

        return query;
    }
}
