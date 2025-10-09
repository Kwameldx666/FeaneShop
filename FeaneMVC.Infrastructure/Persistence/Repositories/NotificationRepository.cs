using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.Interfaces;
using FeaneMVC.Infrastructure.Persistence.Db;
using Microsoft.EntityFrameworkCore;

namespace FeaneMVC.Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public NotificationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ClearFiltersAsync()
    {
        _dbContext.Filters.RemoveRange(_dbContext.Filters);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetAllNotificationsAsync()
    {
        return await _dbContext.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> AddNotificationAsync(string content)
    {
        var notification = new Notification { Content = content };
        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();
        return notification.Id;
    }

    public async Task<List<Filter>> GetAllFiltersAsync()
    {
        return await _dbContext.Filters.ToListAsync();
    }

    public async Task<int> AddFilterAsync(string filterName)
    {
        var filter = new Filter { Name = filterName };
        _dbContext.Filters.Add(filter);
        await _dbContext.SaveChangesAsync();
        return filter.Id;
    }
}
