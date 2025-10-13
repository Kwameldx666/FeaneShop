using System.Net.Http.Json;
using System.Text.Json;
using Feane.Contracts.Dishes;

namespace FeaneMVC.Clients;

internal sealed class MenuServiceClient : IMenuServiceClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public MenuServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IReadOnlyCollection<DishResponse>> GetDishesAsync(CancellationToken cancellationToken = default)
    {
        var dishes = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<DishResponse>>("api/dishes", SerializerOptions, cancellationToken);
        return dishes ?? Array.Empty<DishResponse>();
    }

    public Task<DishResponse?> GetDishAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _httpClient.GetFromJsonAsync<DishResponse>($"api/dishes/{id}", SerializerOptions, cancellationToken);
    }

    public async Task<DishResponse?> CreateDishAsync(CreateDishRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/dishes", request, SerializerOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<DishResponse>(SerializerOptions, cancellationToken);
    }

    public async Task<DishResponse?> UpdateDishAsync(Guid id, UpdateDishRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/dishes/{id}", request, SerializerOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<DishResponse>(SerializerOptions, cancellationToken);
    }

    public async Task<bool> DeleteDishAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/dishes/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<(int created, int skipped)> SeedAsync(int count, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/dishes/seed/{count}", new { }, SerializerOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return (0, 0);
        }

        var payload = await response.Content.ReadFromJsonAsync<SeedResponse>(SerializerOptions, cancellationToken);
        return payload is null ? (0, 0) : (payload.created, payload.skipped);
    }

    private sealed record SeedResponse(int created, int skipped);
}
