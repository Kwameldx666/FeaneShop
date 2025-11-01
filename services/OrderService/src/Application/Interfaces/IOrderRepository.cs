using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetUserOrdersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(Guid orderId, OrderStatus status, CancellationToken cancellationToken = default);
    Task<bool> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}