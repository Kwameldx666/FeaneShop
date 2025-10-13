using System.Net.Http.Json;
using Feane.Contracts.Dishes;

namespace FeaneMVC.Clients.Menu;

public class MenuApiClient : IMenuApiClient
{
    private readonly HttpClient _httpClient;

    public MenuApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<DishResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var dishes = await _httpClient.GetFromJsonAsync<IReadOnlyList<DishResponse>>("api/dishes", cancellationToken);
        return dishes ?? Array.Empty<DishResponse>();
    }

    public async Task<DishResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<DishResponse>($"api/dishes/{id}", cancellationToken);
    }

    public async Task<DishResponse?> CreateAsync(CreateDishRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/dishes", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<DishResponse>(cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateAsync(UpdateDishRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/dishes/{request.Id}", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/dishes/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
