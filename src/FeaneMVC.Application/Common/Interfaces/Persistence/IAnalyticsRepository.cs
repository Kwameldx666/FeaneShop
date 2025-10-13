using FeaneMVC.Domain.Entities;

namespace FeaneMVC.Application.Common.Interfaces.Persistence;

public interface IAnalyticsRepository
{
    Task<int> GetTotalUsersAsync(CancellationToken cancellationToken = default);

    Task<int> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Reservation>> GetReservationsAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentRecord>> GetPaymentsAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentRecord>> GetRefundedPaymentsAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default);
}
