using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IUserProfileClient
{
    Task SyncProfileAsync(User user, CancellationToken cancellationToken = default);
}
