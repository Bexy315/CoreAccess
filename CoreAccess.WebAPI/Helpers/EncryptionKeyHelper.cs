using System.Security.Cryptography;

namespace CoreAccess.WebAPI.Helpers;

public static class EncryptionKeyHelper
{
    private const string KeyPath = "/data/keys/";

    /// <summary>
    /// Gibt einen gespeicherten Base64-kodierten 256-Bit-Schlüssel zurück oder erzeugt einen neuen, falls nicht vorhanden.
    /// </summary>
    /// <param name="keyName">Name des Schlüssels (Dateiname ohne Endung).</param>
    /// <returns>Ein Base64-kodierter 256-Bit-Schlüssel.</returns>
    public static string GetOrCreateBase64Key(string keyName)
    {
        var keyFilePath = Path.Combine(KeyPath, $"{keyName}.key");

        if (File.Exists(keyFilePath))
        {
            return File.ReadAllText(keyFilePath);
        }

        // 256-Bit = 32 Byte zufälliger Schlüssel
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
    /// Gibt einen zufälligen Base64-kodierten 256-Bit-Schlüssel zurück, welcher nicht gespeichert wird.
    /// </summary>
    /// <param name="length">Länge des Schlüssels in Bytes (Standard: 32).</param>
    /// <returns>Ein Base64-kodierter 256-Bit-Schlüssel.</returns>
    public static string GenerateRandomBase64Key(int length = 32)
    {
        var keyBytes = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(keyBytes);
    }
}