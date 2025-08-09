using System.Security.Cryptography;
using DataJuggler.Cryptography;

namespace CoreAccess.WebAPI.Helpers;

public static class SecureKeyHelper
{
    private const string KeyPath = "/data/keys/";

    /// <summary>
    /// Returns a stored Base64-encoded 256-bit key or generates a new one if not present.
    /// </summary>
    /// <param name="keyName">Name of the key (filename without extension).</param>
    /// <returns>A Base64-encoded 256-bit key.</returns>
    public static string GetOrCreateBase64Key(string keyName)
    {
        var keyFilePath = Path.Combine(AppContext.BaseDirectory, KeyPath, $"{keyName}.key");

        if (File.Exists(keyFilePath))
        {
            return File.ReadAllText(keyFilePath);
        }

        // 256-bit = 32 bytes random key
        var keyBytes = RandomNumberGenerator.GetBytes(32);
        var base64Key = Convert.ToBase64String(keyBytes);

        var directory = Path.GetDirectoryName(keyFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(keyFilePath, base64Key);
        return base64Key;
    }
    /// <summary>
    /// Returns a random Base64-encoded 256-bit key which is not stored.
    /// </summary>
    /// <param name="length">Length of the key in bytes (default: 32).</param>
    /// <returns>A Base64-encoded 256-bit key.</returns>
    public static string GenerateRandomBase64Key(int length = 32)
    {
        var keyBytes = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(keyBytes);
    }
    
    /// <summary>
    /// Generates a cryptographically secure random password using a predefined character set,
    /// including uppercase, lowercase, digits, and special characters.
    /// </summary>
    /// <param name="length">The length of the generated password. Defaults to 16 characters.</param>
    /// <returns>A randomly generated secure password as a string.</returns>
    public static string GenerateSecurePassword(int length = 16)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@$%^&*()-_=+";
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }
}