using Microsoft.AspNetCore.Mvc;
using UserService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "user-service", status = "healthy" }));

app.Run();

namespace UserService.Models;

public interface IUserStore
{
    IEnumerable<UserProfile> GetAll();
    UserProfile? GetById(Guid id);
    UserProfile Create(CreateUserRequest request);
}

public sealed class InMemoryUserStore : IUserStore
{
    private readonly Dictionary<Guid, UserProfile> _users = new();

    public IEnumerable<UserProfile> GetAll() => _users.Values;

    public UserProfile? GetById(Guid id) => _users.TryGetValue(id, out var profile) ? profile : null;

    public UserProfile Create(CreateUserRequest request)
    {
        var profile = new UserProfile(Guid.NewGuid(), request.Email, request.DisplayName);
        _users[profile.Id] = profile;
        return profile;
    }
}

public record UserProfile(Guid Id, string Email, string? DisplayName);

public record CreateUserRequest(string Email, string? DisplayName);
