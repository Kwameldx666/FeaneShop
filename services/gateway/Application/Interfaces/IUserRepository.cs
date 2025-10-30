using AuthService.Application.DTOs;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;

namespace AuthService.Application.Interfaces;

public interface IUserRepository
{
    Task<OperationResult<User>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<OperationResult<User>> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
