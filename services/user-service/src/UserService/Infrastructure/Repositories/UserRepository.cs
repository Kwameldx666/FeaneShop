using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Domain.Services;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;
    private readonly ILogger<UserRepository> _logger;
    private readonly INotificationService _notificationService;

    public UserRepository(UserDbContext context, ILogger<UserRepository> logger,
        INotificationService notificationService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationService = notificationService;
    }

    public IEnumerable<UserData> GetAllUsers()
    {
        try
        {
            return _context.Users.AsNoTracking().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving all users.");
            return new List<UserData>();
        }
    }

    public async Task<OperationResult<UserProfile>> GetOneUserByIdAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty) return OperationResult<UserProfile>.Failure("Invalid ID");

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return OperationResult<UserProfile>.Failure("User not found");

            return OperationResult<UserProfile>.Success(new UserProfile { User = user }, "User retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user with ID {UserId}.", id);
            return OperationResult<UserProfile>.Failure("An error occurred while retrieving the user.");
        }
    }

    public OperationResult<UserProfile> AddUser(UserData user)
    {
        try
        {
            if (user == null) return OperationResult<UserProfile>.Failure("User data is null");

            user.Email = user.Email?.Trim() ?? string.Empty;
            user.Username = user.Username?.Trim() ?? string.Empty;
            user.NormalizedEmail = string.IsNullOrWhiteSpace(user.Email) ? null : user.Email.ToUpperInvariant();
            user.NormalizedUserName =
                string.IsNullOrWhiteSpace(user.Username) ? null : user.Username.ToUpperInvariant();

            if (user.Id == Guid.Empty) user.Id = Guid.NewGuid();

            if (_context.Users.Any(u => u.NormalizedEmail == user.NormalizedEmail))
                return OperationResult<UserProfile>.Failure("User with the same email already exists");

            if (!string.IsNullOrWhiteSpace(user.NormalizedUserName) &&
                _context.Users.Any(u => u.NormalizedUserName == user.NormalizedUserName))
                return OperationResult<UserProfile>.Failure("User with the same username already exists");

            user.Password = LoginHelper.HashGen(user.Password);
            user.IsActive = true;
            user.FirstRegisterTime = user.FirstRegisterTime == default ? DateTime.UtcNow : user.FirstRegisterTime;
            user.SecurityStamp ??= Guid.NewGuid().ToString();
            user.ConcurrencyStamp ??= Guid.NewGuid().ToString();

            _context.Users.Add(user);
            _context.SaveChanges();

            return OperationResult<UserProfile>.Success(new UserProfile { User = user }, "User added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a user.");
            return OperationResult<UserProfile>.Failure("An error occurred while adding the user.");
        }
    }

    public async Task<OperationResult<UserProfile>> UpdateUserAsync(Guid id, UserUpdateRequest request)
    {
        try
        {
            var userOld = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (userOld == null) return OperationResult<UserProfile>.Failure("User not found.");

            var newUsername = request.Username?.Trim() ?? string.Empty;
            var newEmail = request.Email?.Trim() ?? string.Empty;
            var newNormalizedUserName = string.IsNullOrWhiteSpace(newUsername) ? null : newUsername.ToUpperInvariant();
            var newNormalizedEmail = string.IsNullOrWhiteSpace(newEmail) ? null : newEmail.ToUpperInvariant();

            // Check for duplicate email (excluding current user)
            if (!string.IsNullOrWhiteSpace(newNormalizedEmail) &&
                newNormalizedEmail != userOld.NormalizedEmail &&
                await _context.Users.AnyAsync(u => u.Id != id && u.NormalizedEmail == newNormalizedEmail))
                return OperationResult<UserProfile>.Failure("User with the same email already exists.");

            // Check for duplicate username (excluding current user)
            if (!string.IsNullOrWhiteSpace(newNormalizedUserName) &&
                newNormalizedUserName != userOld.NormalizedUserName &&
                await _context.Users.AnyAsync(u => u.Id != id && u.NormalizedUserName == newNormalizedUserName))
                return OperationResult<UserProfile>.Failure("User with the same username already exists.");

            userOld.Username = newUsername;
            userOld.Email = newEmail;
            userOld.NormalizedUserName = newNormalizedUserName;
            userOld.NormalizedEmail = newNormalizedEmail;
            userOld.Address = string.IsNullOrWhiteSpace(request.Address) ? userOld.Address : request.Address;
            userOld.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? userOld.PhoneNumber
                : request.PhoneNumber;
            userOld.Roles = request.Role;
            userOld.IsActive = request.IsActive;
            userOld.ConcurrencyStamp = Guid.NewGuid().ToString();

            await _context.SaveChangesAsync();

            return OperationResult<UserProfile>.Success(new UserProfile { User = userOld },
                "User updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating user {UserId}.", id);
            return OperationResult<UserProfile>.Failure("An error occurred while updating the user.");
        }
    }

    public OperationResult<UserProfile> DeleteUser(Guid id)
    {
        try
        {
            if (id == Guid.Empty) return OperationResult<UserProfile>.Failure("Invalid ID");

            var user = _context.Users
                .FirstOrDefault(u => u.Id == id);

            if (user == null) return OperationResult<UserProfile>.Failure("User not found");

            _context.Users.Remove(user);
            _context.SaveChanges();

            return OperationResult<UserProfile>.Success(message: "User deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting user {UserId}.", id);
            return OperationResult<UserProfile>.Failure("An error occurred while deleting the user.");
        }
    }

    public IEnumerable<UserData> FindUsersByName(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name)) return new List<UserData>();

            return _context.Users.AsNoTracking().Where(u => u.Username.Contains(name)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while searching for users with name {Name}.", name);
            return new List<UserData>();
        }
    }

    public OperationResult<UserProfile> AuthenticateUser(string credential, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(credential) || string.IsNullOrWhiteSpace(password))
                return OperationResult<UserProfile>.Failure("Credential or password is invalid");

            var normalizedCredential = credential.Trim();
            var hashedPassword = LoginHelper.HashGen(password);
            var normalizedCredentialUpper = normalizedCredential.ToUpperInvariant();

            var user = _context.Users.SingleOrDefault(u =>
                u.Password == hashedPassword &&
                ((u.NormalizedUserName != null && u.NormalizedUserName == normalizedCredentialUpper) ||
                 (u.NormalizedEmail != null && u.NormalizedEmail == normalizedCredentialUpper) ||
                 u.Username.ToUpper() == normalizedCredentialUpper ||
                 u.Email.ToUpper() == normalizedCredentialUpper));

            if (user == null) return OperationResult<UserProfile>.Failure("Authentication failed");

            return OperationResult<UserProfile>.Success(new UserProfile { User = user },
                "User authenticated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during user authentication for credential {Credential}.",
                credential);
            return OperationResult<UserProfile>.Failure("An error occurred during authentication.");
        }
    }

    public IEnumerable<Role> GetUserRoles(Guid id)
    {
        try
        {
            if (id == Guid.Empty) return Enumerable.Empty<Role>();

            var user = _context.Users.SingleOrDefault(u => u.Id == id);
            return user == null ? Enumerable.Empty<Role>() : new List<Role> { user.Roles };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving roles for user {UserId}.", id);
            return Enumerable.Empty<Role>();
        }
    }

    public OperationResult<UserProfile> ChangeUserPassword(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email)) return OperationResult<UserProfile>.Failure("Email is invalid");

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return OperationResult<UserProfile>.Failure("User not found");

            var password = PasswordGenerator.GeneratePassword();
            _notificationService?.SendNotification($"New password is:{password}", email);

            user.Password = LoginHelper.HashGen(password);
            _context.SaveChanges();

            return OperationResult<UserProfile>.Success(new UserProfile { User = user },
                "Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while changing password for user with email {Email}.", email);
            return OperationResult<UserProfile>.Failure("An error occurred while changing the password.");
        }
    }

    public OperationResult<UserProfile> IsUserExists(Guid id)
    {
        try
        {
            if (id == Guid.Empty) return OperationResult<UserProfile>.Failure("Invalid ID");

            var exists = _context.Users.Any(u => u.Id == id);
            return exists
                ? OperationResult<UserProfile>.Success(new UserProfile(), "User exists")
                : OperationResult<UserProfile>.Failure("User does not exist");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while checking if user {UserId} exists.", id);
            return OperationResult<UserProfile>.Failure("An error occurred while checking the user's existence.");
        }
    }

    public OperationResult<UserProfile> AssignRoleToUser(Guid userId, Role role)
    {
        try
        {
            var user = _context.Users.SingleOrDefault(u => u.Id == userId);
            if (user == null) return OperationResult<UserProfile>.Failure("User not found");

            if (user.Roles == role) return OperationResult<UserProfile>.Failure("Role already assigned to user");

            user.Roles = role;
            _context.SaveChanges();

            return OperationResult<UserProfile>.Success(new UserProfile { User = user }, "Role assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while assigning the role {Role} to user {UserId}.", role, userId);
            return OperationResult<UserProfile>.Failure("An error occurred while assigning the role.");
        }
    }

    public OperationResult<UserProfile> DeactivateUser(Guid id)
    {
        try
        {
            if (id == Guid.Empty) return OperationResult<UserProfile>.Failure("Invalid ID");

            var user = _context.Users.Find(id);
            if (user == null) return OperationResult<UserProfile>.Failure("User not found");

            user.IsActive = false;
            _context.SaveChanges();

            return OperationResult<UserProfile>.Success(new UserProfile { User = user },
                "User deactivated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deactivating user {UserId}.", id);
            return OperationResult<UserProfile>.Failure("An error occurred while deactivating the user.");
        }
    }

    public OperationResult<UserProfile> GetUserData(UserData data)
    {
        try
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == data.Credential || u.Username == data.Credential);
            if (user == null) return OperationResult<UserProfile>.Failure("User not found");

            user.FirstLoginTime = DateTime.UtcNow;
            _context.SaveChanges();

            var isPasswordMatch = data.Password == user.Password;
            var result = OperationResult<UserProfile>.Success(new UserProfile { User = user });
            result.Status = isPasswordMatch;
            if (!isPasswordMatch) result.Message = "Incorrect password";

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user data for credential {Credential}.",
                data.Credential);
            return OperationResult<UserProfile>.Failure("An error occurred while retrieving user data.");
        }
    }

    public async Task<UserData?> GetUserByCookie(string value)
    {
        try
        {
            if (string.IsNullOrEmpty(value)) return null;

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.CookieValue == value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user by cookie value {Cookie}.", value);
            return null;
        }
    }

    public OperationResult UserLogout()
    {
        return OperationResult.Success("User logout acknowledged.");
    }

    public async Task<bool> UpdateUserLoginAuditAsync(Guid userId, string cookieValue, DateTime loginTime,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Cannot update login audit data because the supplied user identifier is empty.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(cookieValue))
            {
                _logger.LogWarning(
                    "Cannot update login audit data for user {UserId} because the generated cookie value is empty.",
                    userId);
                return false;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Cannot update login audit data because user {UserId} was not found.", userId);
                return false;
            }

            user.CookieValue = cookieValue;
            user.FirstLoginTime = loginTime;
            user.IsActive = true;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating login audit data for user {UserId}.", userId);
            return false;
        }
    }
}