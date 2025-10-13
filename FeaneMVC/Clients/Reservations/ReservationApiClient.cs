using System.Net.Http.Json;
using System.Linq;
using Feane.Contracts.Reservations;

namespace FeaneMVC.Clients.Reservations;

public class ReservationApiClient : IReservationApiClient
{
    private readonly HttpClient _httpClient;

    public ReservationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ReservationHistoryPageModel> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/reservations", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(errorMessage) ? "Не удалось создать бронирование" : errorMessage);
        }

        var createdReservation = await response.Content.ReadFromJsonAsync<ReservationHistoryItem>(cancellationToken: cancellationToken);
        if (createdReservation == null)
        {
            throw new InvalidOperationException("Не удалось прочитать ответ сервиса резерваций");
        }

        var history = await GetByUserAsync(request.UserId, cancellationToken);
        history.Items = history.Items.Prepend(createdReservation).DistinctBy(item => item.ReservationId).OrderByDescending(item => item.ReservationDate).ToList();
        return history;
    }

    public async Task<ReservationHistoryPageModel> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var history = await _httpClient.GetFromJsonAsync<IEnumerable<ReservationHistoryItem>>($"api/reservations/user/{userId}", cancellationToken);
        return new ReservationHistoryPageModel
        {
            Items = history?.OrderByDescending(item => item.ReservationDate).ToList() ?? new List<ReservationHistoryItem>()
        };
    }
}
