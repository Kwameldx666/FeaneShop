using System.Text;

namespace UserService.Domain.Services;

public static class PasswordGenerator
{
    private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
    private static readonly Random Random = new();

    public static string GeneratePassword(int length = 12)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Password length must be greater than zero.");

        var result = new StringBuilder(length);
        for (var i = 0; i < length; i++) result.Append(Characters[Random.Next(Characters.Length)]);

        return result.ToString();
    }
}