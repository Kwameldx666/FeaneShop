using FeaneMVC.Domain.Entities;

namespace FeaneMVC.Application.Common.Interfaces.Persistence;

public interface IPaymentRepository
{
    Task<UserData?> FindUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Cart?> FindCartWithItemsAsync(Guid userId, CancellationToken cancellationToken = default);

    void RemoveCart(Cart cart);

    void AddPaymentRecord(PaymentRecord paymentRecord);

    Task<PaymentRecord?> FindPaymentByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);
}
