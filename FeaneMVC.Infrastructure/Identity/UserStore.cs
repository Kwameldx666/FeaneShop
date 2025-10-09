using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.Enums;
using FeaneMVC.Infrastructure.Persistence.Db;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FeaneMVC.Infrastructure.Identity;

public class UserStore :
    IUserStore<UserData>,
    IUserPasswordStore<UserData>,
    IUserEmailStore<UserData>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserStore> _logger;

    public UserStore(ApplicationDbContext context, ILogger<UserStore> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IQueryable<UserData> Users => _context.Users.AsQueryable();

    public void Dispose()
    {
    }

    public async Task<IdentityResult> CreateAsync(UserData user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        NormalizeUser(user);
        user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
        user.SecurityStamp ??= Guid.NewGuid().ToString();
        user.ConcurrencyStamp ??= Guid.NewGuid().ToString();
        user.FirstRegisterTime = user.FirstRegisterTime == default ? DateTime.UtcNow : user.FirstRegisterTime;
        user.IsActive = true;

        var emailExists = await _context.Users.AnyAsync(u => u.NormalizedEmail == user.NormalizedEmail, cancellationToken);
        if (emailExists)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = nameof(IdentityErrorDescriber.DuplicateEmail),
                Description = "Пользователь с таким email уже существует."
            });
        }

        var usernameExists = await _context.Users.AnyAsync(u => u.NormalizedUserName == user.NormalizedUserName, cancellationToken);
        if (usernameExists)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = nameof(IdentityErrorDescriber.DuplicateUserName),
                Description = "Пользователь с таким логином уже существует."
            });
        }

        if (user.CartId == Guid.Empty)
        {
            user.CartId = Guid.NewGuid();
        }

        if (user.DeliveryId == Guid.Empty)
        {
            user.DeliveryId = Guid.NewGuid();
        }

        var cart = new Cart
        {
            CartId = user.CartId,
            UserId = user.Id
        };

        var delivery = new DeliveryAddress
        {
            Id = user.DeliveryId,
            UserId = user.Id
        };

        user.Cart = cart;
        user.Delivery = delivery;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _context.Cart.AddAsync(cart, cancellationToken);
            await _context.DeliveryAddresses.AddAsync(delivery, cancellationToken);
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(exception, "Failed to create user {UserId}", user.Id);
            return IdentityResult.Failed(new IdentityError
            {
                Code = "CreateUserFailed",
                Description = "Не удалось создать пользователя. Попробуйте позже."
            });
        }
    }

    public async Task<IdentityResult> DeleteAsync(UserData user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var existingUser = await _context.Users
            .Include(u => u.Cart)
            .Include(u => u.Delivery)
            .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

        if (existingUser == null)
        {
            return IdentityResult.Success;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (existingUser.Cart != null)
            {
                _context.Cart.Remove(existingUser.Cart);
            }

            if (existingUser.Delivery != null)
            {
                _context.DeliveryAddresses.Remove(existingUser.Delivery);
            }

            _context.Users.Remove(existingUser);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(exception, "Failed to delete user {UserId}", user.Id);
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteUserFailed",
                Description = "Не удалось удалить пользователя."
            });
        }
    }

    public async Task<UserData?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Guid.TryParse(userId, out var id))
        {
            return null;
        }

        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<UserData?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(normalizedUserName))
        {
            return null;
        }

        normalizedUserName = normalizedUserName.ToUpperInvariant();
        return await _context.Users.FirstOrDefaultAsync(
            u => u.NormalizedUserName == normalizedUserName ||
                 (u.NormalizedUserName == null && u.Username.ToUpper() == normalizedUserName),
            cancellationToken);
    }

    public Task<string> GetNormalizedUserNameAsync(UserData user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user?.NormalizedUserName ?? string.Empty);
    }

    public Task<string> GetUserIdAsync(UserData user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string> GetUserNameAsync(UserData user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.Username);
    }

    public Task SetNormalizedUserNameAsync(UserData user, string normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(UserData user, string userName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.Username = userName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(UserData user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        NormalizeUser(user);
        user.ConcurrencyStamp = Guid.NewGuid().ToString();

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public Task SetPasswordHashAsync(UserData user, string passwordHash, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.Password = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(UserData user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.Password);
    }

    public Task<bool> HasPasswordAsync(UserData user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(!string.IsNullOrEmpty(user.Password));
    }

    public Task SetEmailAsync(UserData user, string? email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.Email = email ?? string.Empty;
        user.NormalizedEmail = Normalize(email);
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(UserData user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(UserData user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(UserData user, bool confirmed, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public async Task<UserData?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return null;
        }

        normalizedEmail = normalizedEmail.ToUpperInvariant();
        return await _context.Users.FirstOrDefaultAsync(
            u => u.NormalizedEmail == normalizedEmail ||
                 (u.NormalizedEmail == null && u.Email.ToUpper() == normalizedEmail),
            cancellationToken);
    }

    public Task<string?> GetNormalizedEmailAsync(UserData user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(UserData user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    private static void NormalizeUser(UserData user)
    {
        user.Email = user.Email?.Trim() ?? string.Empty;
        user.Username = user.Username?.Trim() ?? string.Empty;
        user.NormalizedEmail = Normalize(user.Email);
        user.NormalizedUserName = Normalize(user.Username);
        user.Credential = string.IsNullOrWhiteSpace(user.Credential) ? user.Email : user.Credential;
        if (!Enum.IsDefined(typeof(Role), user.Roles))
        {
            user.Roles = Role.User;
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
    }
}
