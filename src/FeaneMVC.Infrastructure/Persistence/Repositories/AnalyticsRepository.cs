using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Infrastructure.Persistence.Db;
using Microsoft.EntityFrameworkCore;

namespace FeaneMVC.Infrastructure.Persistence.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AnalyticsRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<int> GetTotalUsersAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.CountAsync(cancellationToken);
    }

    public Task<int> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.CountAsync(user => user.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<Reservation>> GetReservationsAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Reservations.AsNoTracking().AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(reservation => reservation.ReservationDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(reservation => reservation.ReservationDate < endDate.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentRecord>> GetPaymentsAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PaymentRecords.AsNoTracking();

        if (startDate.HasValue)
        {
            query = query.Where(payment => payment.DateProcessed >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(payment => payment.DateProcessed < endDate.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentRecord>> GetRefundedPaymentsAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PaymentRecords
            .AsNoTracking()
            .Where(payment => payment.IsRefunded && payment.DateRefunded.HasValue);

        if (startDate.HasValue)
        {
            query = query.Where(payment => payment.DateRefunded!.Value >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(payment => payment.DateRefunded!.Value < endDate.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
