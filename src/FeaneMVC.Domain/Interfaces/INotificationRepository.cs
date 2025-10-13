using FeaneMVC.Domain.Entities;

namespace FeaneMVC.Domain.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetAllNotificationsAsync();
    Task<int> AddNotificationAsync(string content);
    Task<List<Filter>> GetAllFiltersAsync();
    Task<int> AddFilterAsync(string filterName);
    Task ClearFiltersAsync();
}
