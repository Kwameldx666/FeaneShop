using AnalyticsService.Domain.Entities;
using AnalyticsService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsService.Infrastructure.Seed;

public static class AnalyticsSeedData
{
    public static async Task SeedAsync(AnalyticsDbContext context)
    {
        // Check if data already exists
        if (await context.OrderStatistics.AnyAsync()) return; // Data already seeded

        var random = new Random();
        var startDate = DateTime.UtcNow.AddDays(-30);

        // Seed Order Statistics for last 30 days
        var orderStatistics = new List<OrderStatistics>();
        for (var i = 0; i < 30; i++)
        {
            var date = startDate.AddDays(i);
            var totalOrders = random.Next(15, 45);
            var completedOrders = (int)(totalOrders * 0.85);
            var cancelledOrders = totalOrders - completedOrders;
            var totalRevenue = (decimal)(totalOrders * random.Next(80, 150));

            orderStatistics.Add(new OrderStatistics
            {
                Id = Guid.NewGuid(),
                Date = date.Date,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                AverageOrderValue = totalRevenue / totalOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.OrderStatistics.AddRangeAsync(orderStatistics);

        // Seed Product Statistics
        var products = new[]
        {
            "Pizza Margherita",
            "Burger Deluxe",
            "Caesar Salad",
            "Pasta Carbonara",
            "Grilled Chicken",
            "Fish & Chips",
            "Vegetable Soup",
            "Chocolate Cake",
            "Ice Cream Sundae",
            "Fresh Juice"
        };

        var productStatistics = new List<ProductStatistics>();

        for (var i = 0; i < 30; i++)
        {
            var date = startDate.AddDays(i);

            foreach (var productName in products.Take(5)) // Top 5 products
            {
                var totalOrders = random.Next(5, 20);
                var totalQuantity = random.Next(totalOrders, totalOrders * 3);
                var unitPrice = random.Next(10, 25);

                productStatistics.Add(new ProductStatistics
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = productName,
                    TotalOrders = totalOrders,
                    TotalQuantitySold = totalQuantity,
                    TotalRevenue = totalQuantity * unitPrice,
                    Date = date.Date,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await context.ProductStatistics.AddRangeAsync(productStatistics);

        // Seed some analytics events
        var events = new List<AnalyticsEvent>();
        for (var i = 0; i < 100; i++)
        {
            var eventTypes = new[] { "OrderCreated", "OrderCompleted", "ProductViewed", "CartUpdated" };
            var entityTypes = new[] { "Order", "Product", "Cart" };

            events.Add(new AnalyticsEvent
            {
                Id = Guid.NewGuid(),
                EventType = eventTypes[random.Next(eventTypes.Length)],
                EntityType = entityTypes[random.Next(entityTypes.Length)],
                EntityId = Guid.NewGuid(),
                Data = $"{{\"amount\": {random.Next(50, 200)}, \"items\": {random.Next(1, 5)}}}",
                UserId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow.AddHours(-random.Next(0, 720))
            });
        }

        await context.AnalyticsEvents.AddRangeAsync(events);

        await context.SaveChangesAsync();
    }
}