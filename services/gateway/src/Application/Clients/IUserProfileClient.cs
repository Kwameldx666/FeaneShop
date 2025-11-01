using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;

namespace AuthService.Application.Clients;

public interface IUserProfileClient
{
    Task<OperationResult> CreateUserProfileAsync(
        User user,
        string plainPassword,
        CancellationToken cancellationToken = default);
}