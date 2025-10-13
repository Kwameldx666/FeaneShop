using System.Collections.Concurrent;
using Feane.Contracts.Reservations;
using ReservationService.Models;
using System.Linq;

namespace ReservationService.Infrastructure;

internal sealed class InMemoryReservationRepository : IReservationRepository
{
    private readonly ConcurrentDictionary<Guid, ReservationDocument> _reservations = new();

    public Task<ReservationDocument> CreateAsync(Guid userId, CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var reservation = new ReservationDocument
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ReservationDateTime = request.ReservationDateTime,
            CustomerName = request.CustomerName,
            PhoneNumber = request.PhoneNumber,
            UserEmail = request.UserEmail,
            NumberOfPeople = request.NumberOfPeople,
            Occasion = request.Occasion,
            SeatingPreference = request.SeatingPreference,
            SpecialRequests = request.SpecialRequests,
            BudgetPerGuest = request.BudgetPerGuest,
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _reservations.TryAdd(reservation.Id, reservation);
        return Task.FromResult(reservation);
    }

    public Task<IReadOnlyCollection<ReservationDocument>> GetUserHistoryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var history = _reservations.Values
            .Where(reservation => reservation.UserId == userId)
            .OrderByDescending(reservation => reservation.ReservationDateTime)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<ReservationDocument>>(history);
    }
}
