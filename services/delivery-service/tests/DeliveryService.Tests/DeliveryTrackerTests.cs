using DeliveryService.Models;

namespace DeliveryService.Tests;

public class DeliveryTrackerTests
{
    [Fact]
    public void CreateInitializesPendingStage()
    {
        var tracker = new InMemoryDeliveryTracker();
        var record = tracker.Create(Guid.NewGuid(), "123 Main Street");

        Assert.Equal(DeliveryStage.PendingPickup, record.Stage);
    }
}
