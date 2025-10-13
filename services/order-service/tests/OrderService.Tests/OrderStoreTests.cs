using OrderService.Models;

namespace OrderService.Tests;

public class OrderStoreTests
{
    [Fact]
    public void CreateOrderSetsPendingStatus()
    {
        var store = new InMemoryOrderStore();
        var order = store.Create(new CreateOrderRequest(Guid.NewGuid(), new[]
        {
            new OrderLine(Guid.NewGuid(), 1, 10m)
        }));

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Single(order.Items);
    }
}
