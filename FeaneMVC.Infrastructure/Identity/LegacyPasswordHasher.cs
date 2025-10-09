using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.Services;
using Microsoft.AspNetCore.Identity;

namespace FeaneMVC.Infrastructure.Identity;

public class LegacyPasswordHasher : IPasswordHasher<UserData>
{
    public string HashPassword(UserData user, string password)
    {
        if (password == null)
        {
            throw new ArgumentNullException(nameof(password));
        }

        return LoginHelper.HashGen(password);
    }

    public PasswordVerificationResult VerifyHashedPassword(UserData user, string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword))
        {
            return PasswordVerificationResult.Failed;
        }

        if (providedPassword == null)
        {
            return PasswordVerificationResult.Failed;
        }

        var providedHash = LoginHelper.HashGen(providedPassword);
        if (string.Equals(hashedPassword, providedHash, StringComparison.OrdinalIgnoreCase))
        {
            return PasswordVerificationResult.Success;
        }

        return PasswordVerificationResult.Failed;
    }
}
