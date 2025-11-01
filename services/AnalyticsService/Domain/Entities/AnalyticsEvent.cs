namespace AnalyticsService.Domain.Entities;

/// <summary>
///     Событие для сбора аналитики (для будущего расширения)
/// </summary>
public class AnalyticsEvent
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty; // OrderCreated, OrderCompleted, ProductViewed, etc.
    public string EntityType { get; set; } = string.Empty; // Order, Product, User
    public Guid? EntityId { get; set; }
    public string Data { get; set; } = string.Empty; // JSON data
    public Guid? UserId { get; set; }
    public DateTime Timestamp { get; set; }
}