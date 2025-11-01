using Microsoft.EntityFrameworkCore;
using ReservationService.Application.DTOs;
using ReservationService.Application.Interfaces;
using ReservationService.Domain.Entities;
using ReservationService.Domain.Enums;
using ReservationService.Infrastructure.Persistence;

namespace ReservationService.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ReservationDbContext _context;
    private readonly ILogger<ReservationRepository> _logger;

    public ReservationRepository(ReservationDbContext context, ILogger<ReservationRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<Reservation>> GetAsync(ReservationQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        options = Normalize(options);

        try
        {
            var query = BuildQuery(options, true);
            var skip = (options.Page - 1) * options.PageSize;

            return await query
                .AsNoTracking()
                .Skip(skip)
                .Take(options.PageSize)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load reservations with options {@Options}", options);
            return Array.Empty<Reservation>();
        }
    }

    public async Task<int> CountAsync(ReservationQueryOptions options, CancellationToken cancellationToken = default)
    {
        options = Normalize(options);

        try
        {
            var query = BuildQuery(options, false);
            return await query.CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count reservations with options {@Options}", options);
            return 0;
        }
    }

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Reservations.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Reservation> AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        if (reservation == null) throw new ArgumentNullException(nameof(reservation));

        reservation.Id = reservation.Id == Guid.Empty ? Guid.NewGuid() : reservation.Id;
        reservation.CreatedAt = DateTime.UtcNow;
        reservation.UpdatedAt = reservation.CreatedAt;
        reservation.Status = reservation.Status == 0 ? ReservationStatus.Pending : reservation.Status;

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);
        return reservation;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, ReservationStatus status,
        CancellationToken cancellationToken = default)
    {
        var current = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (current == null) return false;

        current.Status = status;
        current.UpdatedAt = DateTime.UtcNow;
        current.CancelledAt = status == ReservationStatus.Cancelled ? DateTime.UtcNow : current.CancelledAt;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (entity == null) return false;

        _context.Reservations.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private ReservationQueryOptions Normalize(ReservationQueryOptions? options)
    {
        options ??= new ReservationQueryOptions();
        options.Page = options.Page < 1 ? 1 : options.Page;
        options.PageSize = options.PageSize is < 1 or > 200 ? 25 : options.PageSize;
        return options;
    }

    private IQueryable<Reservation> BuildQuery(ReservationQueryOptions options, bool applyOrdering)
    {
        var query = _context.Reservations.AsQueryable();

        if (options.UserId.HasValue) query = query.Where(r => r.UserId == options.UserId);

        if (!string.IsNullOrWhiteSpace(options.Email))
        {
            var email = options.Email.Trim().ToLowerInvariant();
            query = query.Where(r => r.UserEmail.ToLower() == email);
        }

        if (options.Status.HasValue) query = query.Where(r => r.Status == options.Status.Value);

        if (options.UpcomingOnly)
        {
            var now = DateTime.UtcNow;
            query = query.Where(r => r.ReservationDate >= now);
        }

        if (options.FromDate.HasValue) query = query.Where(r => r.ReservationDate >= options.FromDate.Value);

        if (options.ToDate.HasValue) query = query.Where(r => r.ReservationDate <= options.ToDate.Value);

        if (!applyOrdering) return query;

        var sort = options.Sort?.Trim().ToLowerInvariant();
        var descending = options.Descending;

        query = sort switch
        {
            "date" or "reservationdate" => descending
                ? query.OrderByDescending(r => r.ReservationDate)
                : query.OrderBy(r => r.ReservationDate),
            "created" or "createdat" => descending
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt),
            "updated" or "updatedat" => descending
                ? query.OrderByDescending(r => r.UpdatedAt)
                : query.OrderBy(r => r.UpdatedAt),
            "status" => descending
                ? query.OrderByDescending(r => r.Status)
                : query.OrderBy(r => r.Status),
            _ => query.OrderByDescending(r => r.ReservationDate)
        };

        return query;
    }
}