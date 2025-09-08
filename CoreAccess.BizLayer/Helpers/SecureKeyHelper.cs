using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace CoreAccess.BizLayer.Helpers;

public static class SecureKeyHelper
{
    private const string KeyPath = "/app/data/keys/";
    
    /// <summary>
    /// Loads the specified RSA key or creates a new one if not present.
    /// </summary>
    /// <param name="keyName">Name of the RSA key.</param>
    /// <param name="size">Size of the RSA key in bits (default: 2048).</param>
    /// <returns>An instance of RsaSecurityKey.</returns>
    public static SecurityKey LoadOrCreateRsaKey(string keyName, int size = 2048)
    {
        var path = Path.Combine(KeyPath, $"{keyName}.pem");

        // Stelle sicher, dass das Verzeichnis existiert
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        RSA rsa;

        if (File.Exists(path))
        {
            // Private Key aus PEM laden
            var pem = File.ReadAllText(path);
            rsa = RSA.Create();
            rsa.ImportFromPem(pem);
        }
        else
        {
            // Neuen Key generieren
            rsa = RSA.Create(size);

            // Private Key als PEM exportieren
            var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
            File.WriteAllText(path, privateKeyPem);
        }

        return new RsaSecurityKey(rsa)
        {
            KeyId = keyName
        };
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