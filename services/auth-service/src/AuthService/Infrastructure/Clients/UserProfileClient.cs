using System.Net.Http.Json;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Clients;

public sealed class UserProfileClient : IUserProfileClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserProfileClient> _logger;

    public UserProfileClient(HttpClient httpClient, ILogger<UserProfileClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SyncProfileAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var payload = new
        {
            authUserId = user.Id,
            username = user.Username,
            email = user.Email,
            role = user.Role,
            isActive = user.IsActive
        };

        try
        {
            using var response = await _httpClient.PutAsJsonAsync(
                $"/api/users/{user.Id:D}",
                payload,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Failed to synchronise user profile {UserId}. Status: {Status}. Response: {Body}",
                    user.Id, response.StatusCode, body);
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unable to synchronise user profile {UserId}", user.Id);
        }
    }
}
