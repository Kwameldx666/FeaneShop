using Feane.Contracts.Reservations;
using ReservationService.Models;

namespace ReservationService.Infrastructure;

public interface IReservationRepository
{
    Task<ReservationDocument> CreateAsync(Guid userId, CreateReservationRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ReservationDocument>> GetUserHistoryAsync(Guid userId, CancellationToken cancellationToken);
}
