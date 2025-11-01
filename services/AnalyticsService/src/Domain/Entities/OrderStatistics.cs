namespace AnalyticsService.Domain.Entities;

/// <summary>
///     Агрегированная статистика по заказам за определенный период
/// </summary>
public class OrderStatistics
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}