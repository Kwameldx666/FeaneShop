using System.Security.Cryptography;
using System.Text;

namespace UserService.Domain.Services;

public static class LoginHelper
{
    public static string HashGen(string password)
    {
        using var md5 = MD5.Create();
        var originalBytes = Encoding.UTF8.GetBytes(password + "internship");
        var encodedBytes = md5.ComputeHash(originalBytes);
        return Convert.ToHexString(encodedBytes).ToLowerInvariant();
    }
}