using System.Security.Cryptography;
using System.Text;

namespace FeaneMVC.Application.Common.Security;

public static class CookieGenerator
{
    private const string SaltData = "QADLz4qk3rVgBSGjDfAH3XWVqKKagMXezBPv7TmXvwnXDDeRpHaLBv4JnTGRwLg9tzbmV77g8DUEAEa6JPv66hy7SwHBL4z4FbGdh2MVs4kq9RcaZEAszuP5ccLsEfqCpwdSvVVt479DCZrwjSHrJVwaja9WQaWAmEY9NsPvEHKnFwHTGAvPXpjpCxkbedYquEauLvZLphwmJLUteZ4QAXU6Z4F3PDmh3wsQXvSctQBHvNWf";
    private static readonly byte[] Salt = Encoding.ASCII.GetBytes(SaltData);
    private const string SharedSecret = "BjXNmq5MKKaraLwxz9uaATvFwE4Rj679KguTRE8c2j56FnkuKJKfkGbZEeDGFDvsGYNHpUXFUUUuUHBR4UV3T2kumguhubg6Gpt7CyqGDbUPrMvPc67kX3yP";

    public static string Create(string value)
    {
        return EncryptStringAes(value, SharedSecret);
    }

    public static string Validate(string value)
    {
        return DecryptStringAes(value, SharedSecret);
    }

    private static string EncryptStringAes(string plainText, string sharedSecret)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            throw new ArgumentNullException(nameof(plainText));
        }

        if (string.IsNullOrEmpty(sharedSecret))
        {
            throw new ArgumentNullException(nameof(sharedSecret));
        }

        using var aesAlg = Aes.Create();
        var key = new Rfc2898DeriveBytes(sharedSecret, Salt);
        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        using var msEncrypt = new MemoryStream();
        var iv = aesAlg.IV;
        msEncrypt.Write(BitConverter.GetBytes(iv.Length), 0, sizeof(int));
        msEncrypt.Write(iv, 0, iv.Length);

        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    private static string DecryptStringAes(string cipherText, string sharedSecret)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            throw new ArgumentNullException(nameof(cipherText));
        }

        if (string.IsNullOrEmpty(sharedSecret))
        {
            throw new ArgumentNullException(nameof(sharedSecret));
        }

        var key = new Rfc2898DeriveBytes(sharedSecret, Salt);
        var bytes = Convert.FromBase64String(cipherText);

        using var msDecrypt = new MemoryStream(bytes);
        using var aesAlg = Aes.Create();
        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
        aesAlg.IV = ReadByteArray(msDecrypt);

        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }

    private static byte[] ReadByteArray(Stream stream)
    {
        var rawLength = new byte[sizeof(int)];
        if (stream.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
        {
            throw new SystemException("Stream did not contain properly formatted byte array");
        }

        var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
        if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
        {
            throw new SystemException("Did not read byte array properly");
        }

        return buffer;
    }
}
