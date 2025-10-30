using System.Net.Http.Json;
using System.Text.Json;
using AuthService.Application.Clients;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Services;

public sealed class UserProfileClient : IUserProfileClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserProfileClient> _logger;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public UserProfileClient(HttpClient httpClient, ILogger<UserProfileClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> CreateUserProfileAsync(User user, string plainPassword, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var payload = new UserProvisioningRequest
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Password = plainPassword,
            Roles = user.Role,
            IsActive = user.IsActive,
            FirstRegisterTime = user.FirstRegisterTime
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync("/api/users", payload, _serializerOptions, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to provision user {UserId} in user-service. Status: {StatusCode}. Response: {Response}",
                user.Id, response.StatusCode, error);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision user {UserId} in user-service.", user.Id);
            return false;
        }
    }

    private sealed record UserProvisioningRequest
    {
        public Guid Id { get; init; }
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public AuthService.Domain.Enums.Role Roles { get; init; }
        public bool IsActive { get; init; }
        public DateTime FirstRegisterTime { get; init; }
    }
}
