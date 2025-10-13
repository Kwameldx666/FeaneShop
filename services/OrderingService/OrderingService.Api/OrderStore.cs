namespace OrderingService.Api;

public class OrderStore
{
    private readonly List<Order> _orders = new();

    public IReadOnlyCollection<Order> GetOrders() => _orders;

    public Order CreateOrder(CreateOrderRequest request)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            DeliveryAddress = request.DeliveryAddress,
            Items = request.Items,
            TotalAmount = request.Items.Sum(i => i.Quantity * i.Price),
            Status = OrderStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _orders.Add(order);
        return order;
    }

    public Order? UpdateStatus(Guid orderId, OrderStatus status)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order is null)
        {
            return null;
        }

        order.Status = status;
        order.StatusHistory.Add((DateTimeOffset.UtcNow, status));
        return order;
    }
}

public class Order
{
    public Guid Id { get; init; }
    public required string CustomerName { get; init; }
    public required string DeliveryAddress { get; init; }
    public required IReadOnlyCollection<OrderLine> Items { get; init; }
    public decimal TotalAmount { get; init; }
    public OrderStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public List<(DateTimeOffset Timestamp, OrderStatus Status)> StatusHistory { get; } = new();
}

public record OrderLine(int MenuItemId, string Name, int Quantity, decimal Price);

public record CreateOrderRequest(string CustomerName, string DeliveryAddress, IReadOnlyCollection<OrderLine> Items);

public record UpdateOrderStatusRequest(OrderStatus Status);

public enum OrderStatus
{
    Pending,
    InProgress,
    Ready,
    OutForDelivery,
    Delivered,
    Cancelled
}
