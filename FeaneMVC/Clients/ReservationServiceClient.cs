using System.Net.Http.Json;
using System.Text.Json;
using Feane.Contracts.Reservations;

namespace FeaneMVC.Clients;

internal sealed class ReservationServiceClient : IReservationServiceClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public ReservationServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<ReservationHistoryPageModel> GetHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var history = await _httpClient.GetFromJsonAsync<ReservationHistoryPageModel>($"api/reservations/history/{userId}", SerializerOptions, cancellationToken);
        return history ?? new ReservationHistoryPageModel();
    }

    public async Task<ReservationHistoryItem?> CreateAsync(Guid userId, CreateReservationRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/reservations")
        {
            Content = JsonContent.Create(request, options: SerializerOptions)
        };

        message.Headers.Add("X-User-Id", userId.ToString());

        var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ReservationHistoryItem>(SerializerOptions, cancellationToken);
    }
}
