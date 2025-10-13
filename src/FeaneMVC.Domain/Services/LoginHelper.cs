using System.Security.Cryptography;
using System.Text;

namespace FeaneMVC.Domain.Services
{
    public static class LoginHelper
    {
        public static string HashGen(string password)
        {
            using MD5 md5 = MD5.Create();
            var originalBytes = Encoding.UTF8.GetBytes(password + "internship");
            var encodedBytes = md5.ComputeHash(originalBytes);
            return Convert.ToHexString(encodedBytes).ToLowerInvariant();
        }
    }
}
