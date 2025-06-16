using System.Collections.Concurrent;
using CoreAccess.WebAPI.DbContext;
using CoreAccess.WebAPI.Helpers;
using CoreAccess.WebAPI.Model;
using DataJuggler.Cryptography;

namespace CoreAccess.WebAPI.Services;

public interface IAppSettingsService
{
    string? Get(string key, bool decryptIfNeeded = false);
    bool TryGet<T>(string key, out T? value, bool decryptIfNeeded = false);
    void Reload();
    void Set(string key, string value, bool encrypt = false, bool isSystem = false);
    IReadOnlyCollection<AppSetting> All();
}

public class AppSettingsService : IAppSettingsService
{
    private readonly ConcurrentDictionary<string, AppSetting> _buffer = new();
    private readonly CoreAccessDbContext _db;

    public AppSettingsService(CoreAccessDbContext dbContext)
    {
        _db = dbContext;
        Reload();
    }

    public string? Get(string key, bool decryptIfNeeded = false)
    {
        if (!_buffer.TryGetValue(key, out var entry)) return null;

        return decryptIfNeeded && entry.IsEncrypted
            ? CryptographyHelper.DecryptString(entry.Value, SecureKeyHelper.GetOrCreateBase64Key("settings_encryption"))
            : entry.Value;
    }

    public bool TryGet<T>(string key, out T? value, bool decryptIfNeeded = false)
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

    public void Reload()
    {
        var fresh = _db.AppSettings.ToList();

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

    public void Set(string key, string value, bool encrypt = false, bool isSystem = false)
    {
        var encryptedValue = encrypt ? CryptographyHelper.EncryptString(value, SecureKeyHelper.GetOrCreateBase64Key("settings_encryption")) : value;

        var setting = _db.AppSettings.FirstOrDefault(x => x.Key == key);
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
            _db.AppSettings.Add(setting);
        }
        else
        {
            if (setting.IsSystem)
                throw new InvalidOperationException("System-Settings können nicht überschrieben werden.");

            setting.Value = encryptedValue;
            setting.IsEncrypted = encrypt;
        }

        _db.SaveChanges();
        Reload();
    }

    public IReadOnlyCollection<AppSetting> All() => _buffer.Values.ToList();
}