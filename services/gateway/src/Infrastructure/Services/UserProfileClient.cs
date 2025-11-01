using System.Text.Json;
using AuthService.Application.Clients;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.ValueObjects;

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

    public async Task<OperationResult> CreateUserProfileAsync(
        User user,
        string plainPassword,
        CancellationToken cancellationToken = default)
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
            using var response =
                await _httpClient.PostAsJsonAsync("/api/users", payload, _serializerOptions, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var provisionResult = TryParseProvisioningResponse(body);

            if (response.IsSuccessStatusCode) return OperationResult.Success(provisionResult?.Message);

            var message = !string.IsNullOrWhiteSpace(provisionResult?.Message)
                ? provisionResult!.Message!
                : $"User-service responded with {(int)response.StatusCode} {response.ReasonPhrase}.";

            _logger.LogWarning(
                "Failed to provision user {UserId} in user-service. Status: {StatusCode}. Response: {Response}",
                user.Id,
                response.StatusCode,
                string.IsNullOrWhiteSpace(body) ? "<empty>" : body);

            return OperationResult.Failure(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision user {UserId} in user-service.", user.Id);
            return OperationResult.Failure("User provisioning request failed.");
        }
    }

    private ProvisionResponse? TryParseProvisioningResponse(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return null;

        try
        {
            return JsonSerializer.Deserialize<ProvisionResponse>(payload, _serializerOptions);
        }
        catch (JsonException jsonException)
        {
            _logger.LogDebug(jsonException, "Unable to deserialize user-service response: {Payload}", payload);
            return null;
        }
    }

    private sealed record UserProvisioningRequest
    {
        public Guid Id { get; init; }
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public Role Roles { get; init; }
        public bool IsActive { get; init; }
        public DateTime FirstRegisterTime { get; init; }
    }

    private sealed record ProvisionResponse
    {
        public bool Status { get; init; }
        public string? Message { get; init; }
    }
}