using AuthService.Application.Clients;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Services;
using AuthService.Domain.ValueObjects;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;
    private readonly ILogger<UserRepository> _logger;
    private readonly IUserProfileClient _userProfileClient;

    public UserRepository(AuthDbContext context, ILogger<UserRepository> logger, IUserProfileClient userProfileClient)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userProfileClient = userProfileClient ?? throw new ArgumentNullException(nameof(userProfileClient));
    }

    public async Task<OperationResult<User>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var normalizedEmail = request.Email.Trim().ToUpperInvariant();
            var normalizedUserName = request.Username.Trim().ToUpperInvariant();

            if (await _context.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken))
            {
                return OperationResult<User>.Failure("User with the same email already exists");
            }

            if (await _context.Users.AnyAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken))
            {
                return OperationResult<User>.Failure("User with the same username already exists");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username.Trim(),
                NormalizedUserName = normalizedUserName,
                Email = request.Email.Trim(),
                NormalizedEmail = normalizedEmail,
                Password = LoginHelper.HashGen(request.Password),
                Role = request.Role,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                IsActive = true,
                FirstRegisterTime = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var profileCreated = await _userProfileClient.CreateUserProfileAsync(user, request.Password, cancellationToken);
            if (!profileCreated)
            {
                _logger.LogWarning("Rolling back auth user creation because user-service provisioning failed for {Email}", request.Email);
                _context.Users.Remove(user);
                await _context.SaveChangesAsync(cancellationToken);
                return OperationResult<User>.Failure("Failed to create user profile.");
            }

            return OperationResult<User>.Success(user, "User registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register user {Email}", request.Email);
            return OperationResult<User>.Failure("An error occurred while registering the user.");
        }
    }

    public async Task<OperationResult<User>> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var normalizedCredential = request.Credential.Trim().ToUpperInvariant();
            var hashedPassword = LoginHelper.HashGen(request.Password);

            var user = await _context.Users.SingleOrDefaultAsync(
                u => u.Password == hashedPassword &&
                     (u.NormalizedUserName == normalizedCredential ||
                      u.NormalizedEmail == normalizedCredential),
                cancellationToken);

            if (user == null)
            {
                return OperationResult<User>.Failure("Authentication failed");
            }

            if (!user.IsActive)
            {
                return OperationResult<User>.Failure("User account is inactive");
            }

            user.FirstLoginTime ??= DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult<User>.Success(user, "User authenticated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to authenticate user {Credential}", request.Credential);
            return OperationResult<User>.Failure("An error occurred during authentication.");
        }
    }

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }
}
