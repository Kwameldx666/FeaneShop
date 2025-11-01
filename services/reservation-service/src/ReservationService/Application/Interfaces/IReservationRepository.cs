using ReservationService.Application.DTOs;
using ReservationService.Domain.Entities;
using ReservationService.Domain.Enums;

namespace ReservationService.Application.Interfaces;

public interface IReservationRepository
{
    Task<IReadOnlyList<Reservation>> GetAsync(ReservationQueryOptions options,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(ReservationQueryOptions options, CancellationToken cancellationToken = default);
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Reservation> AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(Guid id, ReservationStatus status, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}