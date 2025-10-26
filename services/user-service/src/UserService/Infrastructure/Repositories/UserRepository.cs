using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(UserDbContext context, ILogger<UserRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            if (id == Guid.Empty)
            {
                return OperationResult<UserProfile>.Failure("Invalid ID");
            }

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Delivery)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return OperationResult<UserProfile>.Failure("User not found");
            }

            return OperationResult<UserProfile>.Success(new UserProfile { User = user, DeliveryAddress = user.Delivery }, "User retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user with ID {UserId}.", id);
            return OperationResult<UserProfile>.Failure("An error occurred while retrieving the user.");
        }
    }

    public async Task<OperationResult<UserProfile>> UpdateAddress(Guid userId, DeliveryAddress newAddress)
    {
        try
        {
            if (newAddress == null)
            {
                return OperationResult<UserProfile>.Failure("No new data provided.");
            }

            var delivery = await _context.DeliveryAddresses.SingleOrDefaultAsync(d => d.UserId == userId);
            if (delivery == null)
            {
                return OperationResult<UserProfile>.Failure("Address not found.");
            }

            if (!string.IsNullOrEmpty(newAddress.MoreInfo))
            {
                delivery.MoreInfo = newAddress.MoreInfo;
            }

            if (!string.IsNullOrEmpty(newAddress.City))
            {
                delivery.City = newAddress.City;
            }

            if (!string.IsNullOrEmpty(newAddress.Street))
            {
                delivery.Street = newAddress.Street;
            }

            if (!string.IsNullOrEmpty(newAddress.Country))
            {
                delivery.Country = newAddress.Country;
            }

            if (!string.IsNullOrEmpty(newAddress.ParcelIndex))
            {
                delivery.ParcelIndex = newAddress.ParcelIndex;
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.Delivery = delivery;
            }

            await _context.SaveChangesAsync();

            return OperationResult<UserProfile>.Success(new UserProfile { User = user, DeliveryAddress = delivery }, "Address updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the address for user {UserId}.", userId);
            return OperationResult<UserProfile>.Failure("An error occurred while updating the address.");
        }
    }

    public async Task<OperationResult<DeliveryAddress>> GetOneAddressByUserIdAsync(Guid userId)
    {
        try
        {
            var delivery = await _context.DeliveryAddresses.AsNoTracking().FirstOrDefaultAsync(d => d.UserId == userId);
            if (delivery == null)
            {
                return OperationResult<DeliveryAddress>.Failure("Address not found.");
            }

            return OperationResult<DeliveryAddress>.Success(delivery, "Address retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the address for user {UserId}.", userId);
            return OperationResult<DeliveryAddress>.Failure("An error occurred while retrieving the address.");
        }
    }

    public async Task<OperationResult<UserProfile>> UpdateUser(UserData userNew)
    {
        try
        {
            var userId = userNew.Id != Guid.Empty ? userNew.Id : userNew.AuthUserId;
            if (userId == Guid.Empty)
            {
                return OperationResult<UserProfile>.Failure("User identifier is required.");
            }

            var userOld = await _context.Users
                .Include(u => u.Delivery)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (userOld == null)
            {
                userOld = new UserData
                {
                    Id = userId,
                    AuthUserId = userNew.AuthUserId == Guid.Empty ? userId : userNew.AuthUserId,
                    FirstRegisterTime = userNew.FirstRegisterTime == default ? DateTime.UtcNow : userNew.FirstRegisterTime,
                    IsActive = userNew.IsActive,
                    Roles = userNew.Roles
                };

                var delivery = new DeliveryAddress
                {
                    Id = Guid.NewGuid(),
                    UserId = userOld.Id
                };

                userOld.DeliveryId = delivery.Id;
                userOld.Delivery = delivery;

                await _context.DeliveryAddresses.AddAsync(delivery);
                await _context.Users.AddAsync(userOld);
            }

            userOld.Username = userNew.Username?.Trim() ?? string.Empty;
            userOld.Email = userNew.Email?.Trim() ?? string.Empty;
            userOld.NormalizedUserName = string.IsNullOrWhiteSpace(userOld.Username) ? null : userOld.Username.ToUpperInvariant();
            userOld.NormalizedEmail = string.IsNullOrWhiteSpace(userOld.Email) ? null : userOld.Email.ToUpperInvariant();
            userOld.Address = userNew.Address;
            userOld.PhoneNumber = userNew.PhoneNumber;
            userOld.Roles = userNew.Roles;
            userOld.IsActive = userNew.IsActive;
            userOld.AuthUserId = userNew.AuthUserId == Guid.Empty ? userOld.AuthUserId : userNew.AuthUserId;

            await _context.SaveChangesAsync();

            return OperationResult<UserProfile>.Success(new UserProfile { User = userOld, DeliveryAddress = userOld.Delivery }, "User updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating user {UserId}.", userNew.Id);
            return OperationResult<UserProfile>.Failure("An error occurred while updating the user.");
        }
    }

    public OperationResult<UserProfile> DeleteUser(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return OperationResult<UserProfile>.Failure("Invalid ID");
            }

            var user = _context.Users
                .Include(u => u.Delivery)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return OperationResult<UserProfile>.Failure("User not found");
            }

            if (user.Delivery != null)
            {
                _context.DeliveryAddresses.Remove(user.Delivery);
            }

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
            if (string.IsNullOrWhiteSpace(name))
            {
                return new List<UserData>();
            }

            return _context.Users.AsNoTracking().Where(u => u.Username.Contains(name)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while searching for users with name {Name}.", name);
            return new List<UserData>();
        }
    }

    public IEnumerable<Role> GetUserRoles(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return Enumerable.Empty<Role>();
            }

            var user = _context.Users.SingleOrDefault(u => u.Id == id);
            return user == null ? Enumerable.Empty<Role>() : new List<Role> { user.Roles };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving roles for user {UserId}.", id);
            return Enumerable.Empty<Role>();
        }
    }

    public OperationResult<UserProfile> IsUserExists(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return OperationResult<UserProfile>.Failure("Invalid ID");
            }

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
            if (user == null)
            {
                return OperationResult<UserProfile>.Failure("User not found");
            }

            if (user.Roles == role)
            {
                return OperationResult<UserProfile>.Failure("Role already assigned to user");
            }

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
            if (id == Guid.Empty)
            {
                return OperationResult<UserProfile>.Failure("Invalid ID");
            }

            var user = _context.Users.Find(id);
            if (user == null)
            {
                return OperationResult<UserProfile>.Failure("User not found");
            }

            user.IsActive = false;
            _context.SaveChanges();

            return OperationResult<UserProfile>.Success(new UserProfile { User = user }, "User deactivated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deactivating user {UserId}.", id);
            return OperationResult<UserProfile>.Failure("An error occurred while deactivating the user.");
        }
    }

    // Legacy authentication helpers removed - user-service no longer manages credentials.
}
