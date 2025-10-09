using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.Enums;
using FeaneMVC.Domain.Interfaces;
using FeaneMVC.Domain.Services;
using FeaneMVC.Domain.ValueObjects;
using FeaneMVC.Infrastructure.Persistence.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FeaneMVC.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRepository> _logger;
        private readonly INotification _notification;

        public UserRepository(IHttpContextAccessor httpContextAccessor, INotification notification, ApplicationDbContext context, ILogger<UserRepository> logger)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notification = notification;
        }

        public IEnumerable<UserData> GetAllUsers()
        {
            try
            {
                var users = _context.Users.ToList();
                return users ?? new List<UserData>();
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

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return OperationResult<UserProfile>.Failure("User not found");
                }

                return OperationResult<UserProfile>.Success(new UserProfile { User = user }, "User retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving user with ID: {id}");
                return OperationResult<UserProfile>.Failure("An error occurred while retrieving the user.");
            }
        }

        public OperationResult<UserProfile> AddUser(UserData user)
        {
            try
            {
                if (user == null)
                {
                    return OperationResult<UserProfile>.Failure("User data is null");
                }

                user.Email = user.Email?.Trim() ?? string.Empty;
                user.Username = user.Username?.Trim() ?? string.Empty;
                user.NormalizedEmail = string.IsNullOrWhiteSpace(user.Email) ? null : user.Email.ToUpperInvariant();
                user.NormalizedUserName = string.IsNullOrWhiteSpace(user.Username) ? null : user.Username.ToUpperInvariant();

                if (user.Id == Guid.Empty)
                {
                    user.Id = Guid.NewGuid();
                }

                if (_context.Users.Any(u => u.NormalizedEmail == user.NormalizedEmail))
                {
                    return OperationResult<UserProfile>.Failure("User with the same email already exists");
                }

                if (!string.IsNullOrWhiteSpace(user.NormalizedUserName) &&
                    _context.Users.Any(u => u.NormalizedUserName == user.NormalizedUserName))
                {
                    return OperationResult<UserProfile>.Failure("User with the same username already exists");
                }

                user.Password = LoginHelper.HashGen(user.Password);
                user.IsActive = true;
                user.FirstRegisterTime = user.FirstRegisterTime == default ? DateTime.UtcNow : user.FirstRegisterTime;
                user.SecurityStamp ??= Guid.NewGuid().ToString();
                user.ConcurrencyStamp ??= Guid.NewGuid().ToString();

                var cart = new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = user.Id
                };

                var deliveryAddress = new DeliveryAddress
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id
                };

                user.DeliveryId = deliveryAddress.Id;
                user.CartId = cart.CartId;
                user.Delivery = deliveryAddress;
                user.Cart = cart;

                _context.Cart.Add(cart);
                _context.DeliveryAddresses.Add(deliveryAddress);
                _context.Users.Add(user);
                _context.SaveChanges();

                return OperationResult<UserProfile>.Success(new UserProfile { User = user, DeliveryAddress = deliveryAddress }, "User added successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                return OperationResult<UserProfile>.Failure("An error occurred while adding the user.");
            }
        }

        public async Task<OperationResult<UserProfile>> UpdateAddress(UserData addressOld, DeliveryAddress newAddress)
        {
            try
            {
                if (newAddress == null)
                {
                    return OperationResult<UserProfile>.Failure("No new data provided.");
                }

                var delivery = await _context.DeliveryAddresses.SingleOrDefaultAsync(d => d.UserId == addressOld.Id);
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

                var user = await _context.Users.SingleOrDefaultAsync(U => U.Id == addressOld.Id);
                if (user != null)
                {
                    user.Delivery = delivery;
                }

                await _context.SaveChangesAsync();

                return OperationResult<UserProfile>.Success(new UserProfile { User = user, DeliveryAddress = delivery }, "Address updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the address.");
                return OperationResult<UserProfile>.Failure("An error occurred while updating the address.");
            }
        }

        public async Task<OperationResult<DeliveryAddress>> GetOneAddressByUserIdAsync(Guid userId)
        {
            try
            {
                var delivery = await _context.DeliveryAddresses.FirstOrDefaultAsync(d => d.UserId == userId);
                if (delivery == null)
                {
                    return OperationResult<DeliveryAddress>.Failure("Address not found.");
                }

                return OperationResult<DeliveryAddress>.Success(delivery, "Address retrieved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                return OperationResult<DeliveryAddress>.Failure("An error occurred while retrieving the address.");
            }
        }

        public async Task<OperationResult<UserProfile>> UpdateUser(UserData userNew)
        {
            try
            {
                var userOld = await _context.Users.FirstOrDefaultAsync(u => u.Id == userNew.Id);
                if (userOld == null)
                {
                    return OperationResult<UserProfile>.Failure("User not found.");
                }

                userOld.Username = userNew.Username?.Trim() ?? string.Empty;
                userOld.Email = userNew.Email?.Trim() ?? string.Empty;
                userOld.NormalizedUserName = string.IsNullOrWhiteSpace(userOld.Username) ? null : userOld.Username.ToUpperInvariant();
                userOld.NormalizedEmail = string.IsNullOrWhiteSpace(userOld.Email) ? null : userOld.Email.ToUpperInvariant();
                userOld.Address = userNew.Address;
                userOld.PhoneNumber = userNew.PhoneNumber;
                userOld.Roles = userNew.Roles;
                userOld.ConcurrencyStamp = Guid.NewGuid().ToString();

                await _context.SaveChangesAsync();

                return OperationResult<UserProfile>.Success(new UserProfile { User = userOld }, "User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating user with ID: {userNew.Id}");
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
                    .Include(u => u.Cart)
                    .Include(u => u.Delivery)
                    .FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    return OperationResult<UserProfile>.Failure("User not found");
                }

                if (user.Cart != null)
                {
                    _context.Cart.Remove(user.Cart);
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
                _logger.LogError(ex, $"An error occurred while deleting user with ID: {id}");
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

                return _context.Users.Where(u => u.Username.Contains(name)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while searching for users with name: {name}");
                return new List<UserData>();
            }
        }

        public OperationResult<UserProfile> AuthenticateUser(string credential, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(credential) || string.IsNullOrWhiteSpace(password))
                {
                    return OperationResult<UserProfile>.Failure("Credential or password is invalid");
                }

                var normalizedCredential = credential.Trim();
                var hashedPassword = LoginHelper.HashGen(password);
                var normalizedCredentialUpper = normalizedCredential.ToUpperInvariant();

                var user = _context.Users.SingleOrDefault(u =>
                    u.Password == hashedPassword &&
                    ((u.NormalizedUserName != null && u.NormalizedUserName == normalizedCredentialUpper) ||
                     (u.NormalizedEmail != null && u.NormalizedEmail == normalizedCredentialUpper) ||
                     u.Username.ToUpper() == normalizedCredentialUpper ||
                     u.Email.ToUpper() == normalizedCredentialUpper));

                if (user == null)
                {
                    return OperationResult<UserProfile>.Failure("Authentication failed");
                }

                var session = _httpContextAccessor.HttpContext?.Session;
                session?.SetString("UserId", user.Id.ToString());
                session?.SetString("UserRole", user.Roles.ToString());

                return OperationResult<UserProfile>.Success(new UserProfile { User = user }, "User authenticated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user authentication.");
                return OperationResult<UserProfile>.Failure("An error occurred during authentication.");
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
                if (user == null)
                {
                    return Enumerable.Empty<Role>();
                }

                return new List<Role> { user.Roles };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving roles for user with ID: {id}");
                return Enumerable.Empty<Role>();
            }
        }

        public OperationResult<UserProfile> ChangeUserPassword(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return OperationResult<UserProfile>.Failure("Email is invalid");
                }

                var user = _context.Users.FirstOrDefault(u => u.Email == email);
                if (user == null)
                {
                    return OperationResult<UserProfile>.Failure("User not found");
                }

                var password = PasswordGenerator.GeneratePassword();
                _notification.SendNotification($"New password is:{password}", email);

                user.Password = LoginHelper.HashGen(password);
                _context.SaveChanges();

                return OperationResult<UserProfile>.Success(new UserProfile { User = user }, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while changing password for user with email: {email}");
                return OperationResult<UserProfile>.Failure("An error occurred while changing the password.");
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
                _logger.LogError(ex, $"An error occurred while checking if user with ID: {id} exists.");
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
                _logger.LogError(ex, $"An error occurred while assigning role to user with ID: {userId}");
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
                _logger.LogError(ex, $"An error occurred while deactivating user with ID: {id}");
                return OperationResult<UserProfile>.Failure("An error occurred while deactivating the user.");
            }
        }

        public OperationResult<UserProfile> GetUserData(UserData data)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == data.Credential || u.Username == data.Credential);
                if (user == null)
                {
                    return OperationResult<UserProfile>.Failure("User not found");
                }

                user.FirstLoginTime = DateTime.Now;
                _context.SaveChanges();

                var isPasswordMatch = data.Password == user.Password;
                var result = OperationResult<UserProfile>.Success(new UserProfile { User = user });
                result.Status = isPasswordMatch;
                if (!isPasswordMatch)
                {
                    result.Message = "Incorrect password";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving user data for credential: {data.Credential}");
                return OperationResult<UserProfile>.Failure("An error occurred while retrieving user data.");
            }
        }

        public async Task<UserData> GetUserByCookie(string cookieValue)
        {
            try
            {
                if (string.IsNullOrEmpty(cookieValue))
                {
                    return null;
                }

                return await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.CookieValue == cookieValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving user by cookie value: {cookieValue}");
                return null;
            }
        }

        public OperationResult UserLogout()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                httpContext.Session.Clear();

                if (httpContext.Request.Cookies.ContainsKey("UserCookie"))
                {
                    httpContext.Response.Cookies.Delete("UserCookie");
                }

                return OperationResult.Success("User successfully logged out.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging out the user.");
                return OperationResult.Failure("An error occurred while logging out the user.");
            }
        }

        public async Task<bool> UpdateUserLoginAuditAsync(Guid userId, string cookieValue, DateTime loginTime, CancellationToken cancellationToken = default)
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
                    _logger.LogWarning("Cannot update login audit data for user {UserId} because the generated cookie value is empty.", userId);
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
}
