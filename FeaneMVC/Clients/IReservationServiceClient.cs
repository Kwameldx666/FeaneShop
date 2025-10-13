using Feane.Contracts.Reservations;

namespace FeaneMVC.Clients;

public interface IReservationServiceClient
{
    Task<ReservationHistoryPageModel> GetHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ReservationHistoryItem?> CreateAsync(Guid userId, CreateReservationRequest request, CancellationToken cancellationToken = default);
}
