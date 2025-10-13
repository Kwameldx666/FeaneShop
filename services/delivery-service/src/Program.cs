using Microsoft.AspNetCore.Mvc;
using DeliveryService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IDeliveryTracker, InMemoryDeliveryTracker>();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "delivery-service", status = "healthy" }));

app.Run();

namespace DeliveryService.Models;

public interface IDeliveryTracker
{
    DeliveryStatusRecord Create(Guid orderId, string address);
    DeliveryStatusRecord? Get(Guid orderId);
    DeliveryStatusRecord Update(Guid orderId, DeliveryStage stage);
}

public sealed class InMemoryDeliveryTracker : IDeliveryTracker
{
    private readonly Dictionary<Guid, DeliveryStatusRecord> _records = new();

    public DeliveryStatusRecord Create(Guid orderId, string address)
    {
        var record = new DeliveryStatusRecord(orderId, address, DeliveryStage.PendingPickup, DateTimeOffset.UtcNow);
        _records[orderId] = record;
        return record;
    }

    public DeliveryStatusRecord? Get(Guid orderId) => _records.TryGetValue(orderId, out var record) ? record : null;

    public DeliveryStatusRecord Update(Guid orderId, DeliveryStage stage)
    {
        if (!_records.TryGetValue(orderId, out var existing))
        {
            throw new KeyNotFoundException($"Delivery for order {orderId} not found");
        }

        var updated = existing with { Stage = stage, UpdatedAt = DateTimeOffset.UtcNow };
        _records[orderId] = updated;
        return updated;
    }
}

public record DeliveryStatusRecord(Guid OrderId, string Address, DeliveryStage Stage, DateTimeOffset UpdatedAt);

public enum DeliveryStage
{
    PendingPickup,
    InTransit,
    Delivered,
    Failed
}
