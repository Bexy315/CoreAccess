using System.Collections.Concurrent;
using CoreAccess.WebAPI.DbContext;
using CoreAccess.WebAPI.Model;
using DataJuggler.Cryptography;

namespace CoreAccess.WebAPI.Helpers;

public static class AppSettingsHelper
{
    private static readonly ConcurrentDictionary<string, AppSetting> _buffer = new();
    private static IServiceProvider _serviceProvider;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Reload();
    }

    public static string? Get(string key, bool decryptIfNeeded = false)
    {
        if (!_buffer.TryGetValue(key, out var entry)) return null;

        return decryptIfNeeded && entry.IsEncrypted
            ? CryptographyHelper.DecryptString(entry.Value, SecureKeyHelper.GetOrCreateBase64Key("settings_encryption"))
            : entry.Value;
    }

    public static bool TryGet<T>(string key, out T? value, bool decryptIfNeeded = false)
    {
        value = default;
        if (!_buffer.TryGetValue(key, out var entry)) return false;

        try
        {
            var raw = decryptIfNeeded && entry.IsEncrypted
                ? CryptographyHelper.DecryptString(entry.Value, SecureKeyHelper.GetOrCreateBase64Key("settings_encryption"))
                : entry.Value;

            value = (T?)Convert.ChangeType(raw, typeof(T));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void Reload()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoreAccessDbContext>();

        var fresh = db.AppSettings
            .ToList();

        _buffer.Clear();
        foreach (var entry in fresh)
        {
            _buffer[entry.Key] = new AppSetting
            {
                Id = entry.Id,
                Key = entry.Key,
                Value = entry.Value,
                IsEncrypted = entry.IsEncrypted,
                IsSystem = entry.IsSystem
            };
        }
    }

    public static void Set(string key, string value, bool encrypt = false, bool isSystem = false)
    {
        var encryptedValue = encrypt ? CryptographyHelper.EncryptString(value, SecureKeyHelper.GetOrCreateBase64Key("settings_encryption")) : value;

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoreAccessDbContext>();

        var setting = db.AppSettings.FirstOrDefault(x => x.Key == key);
        if (setting is null)
        {
            setting = new AppSetting
            {
                Id = Guid.NewGuid(),
                Key = key,
                Value = encryptedValue,
                IsEncrypted = encrypt,
                IsSystem = isSystem
            };
            db.AppSettings.Add(setting);
        }
        else
        {
            if (setting.IsSystem)
                throw new InvalidOperationException("System-Settings können nicht überschrieben werden.");

            setting.Value = encryptedValue;
            setting.IsEncrypted = encrypt;
        }

        db.SaveChanges();
        Reload();
    }

    public static IReadOnlyCollection<AppSetting> All() => _buffer.Values.ToList();
}