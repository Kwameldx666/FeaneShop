using Feane.Contracts.Reservations;

namespace FeaneMVC.Clients.Reservations;

public interface IReservationApiClient
{
    Task<ReservationHistoryPageModel> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default);
    Task<ReservationHistoryPageModel> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
