using AuthService.Domain.Entities;

namespace AuthService.Application.Clients;

public interface IUserProfileClient
{
    Task<bool> CreateUserProfileAsync(User user, string plainPassword, CancellationToken cancellationToken = default);
}
