using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CoreAccess.BizLayer.Services;

public interface ISettingsService
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, string value, bool isSecret, CancellationToken cancellationToken = default);
    Task<int> GetTokenLifetimeAsync(CancellationToken cancellationToken = default);
}

public class SettingsService(ISettingsRepository settingsRepository, ISecretProtector protector, IMemoryCache cache)
    : ISettingsService
{
    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(key, out string? value)) return value;

        var s = await settingsRepository.SearchSettingsAsync(new SettingSearchOptions(){Key = key}, cancellationToken).ContinueWith(x => x.Result.FirstOrDefault(), cancellationToken);
        if (s == null) return null;

        value = s.IsSecret ? protector.Unprotect(s.EncryptedValue!) : s.Value!;
        cache.Set(key, value, TimeSpan.FromMinutes(5));
        return value;
    }

    public async Task SetAsync(string key, string value, bool isSecret, CancellationToken cancellationToken = default)
    {
        var s = await settingsRepository.SearchSettingsAsync(new SettingSearchOptions(){Key = key}, cancellationToken).ContinueWith(x => x.Result.FirstOrDefault(), cancellationToken) ?? new Setting { Key = key };
        if (isSecret)
        {
            s.IsSecret = true;
            s.EncryptedValue = protector.Protect(value);
            s.Value = null;
        }
        else
        {
            s.IsSecret = false;
            s.Value = value;
            s.EncryptedValue = null;
        }
        s.UpdatedAt = DateTime.UtcNow;

        await settingsRepository.InsertOrUpdateSettingAsync(s, cancellationToken);
        cache.Remove(key);
    }

    public async Task<int> GetTokenLifetimeAsync(CancellationToken cancellationToken = default)
    {
        var raw = await GetAsync("TokenLifetime", cancellationToken);
        return int.TryParse(raw, out var sec) ? sec : 3600;
    }
}
