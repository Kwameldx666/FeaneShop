using System.Text;

namespace FeaneMVC.Domain.Services
{
    public static class PasswordGenerator
    {
        private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
        private static readonly Random Random = new();

        public static string GeneratePassword(int length = 12)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Password length must be greater than zero.");
            }

            StringBuilder result = new(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(Characters[Random.Next(Characters.Length)]);
            }

            return result.ToString();
        }
    }
}
