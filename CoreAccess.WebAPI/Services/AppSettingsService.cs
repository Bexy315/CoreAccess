using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Repositories;

namespace CoreAccess.WebAPI.Services;

public interface IAppSettingsService
{
    public Task<AppSettingDto?> SearchSettingAsync(AppSettingSearchOptions options);
    public Task<AppSettingDto> CreateAppSetting(AppSettingCreateRequest request);
    public Task<AppSettingDto> UpdateAppSetting(string id, AppSettingUpdateRequest request);
    public Task DeleteSettingAsync(string key);
    public Task<string> GetDecrypted(string key);
    public Task SetEncrypted(string key, string value);
}

public class AppSettingsService(IAppSettingsRepository repository, IDataEncryptionService encryptionService) : IAppSettingsService
{
    public async Task<AppSettingDto?> SearchSettingAsync(AppSettingSearchOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options), "Search options cannot be null");
        }
        try
        {
            var setting = await repository.SearchSettingsAsync(options);
            return setting == null ? null : new AppSettingDto(setting);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<AppSettingDto> CreateAppSetting(AppSettingCreateRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Setting cannot be null");
        }
        try
        {
            if(!string.IsNullOrEmpty(request.Value) && request.IsEncrypted)
            {
                request.Value = encryptionService.Encrypt(request.Value);
            }
            
            var appSetting = new AppSetting
            {
                Key = request.Key,
                Value = request.Value,
                IsEncrypted = request.IsEncrypted,
                IsSystem = request.IsSystem
            };

            var createdSetting = await repository.InsertOrUpdateSettingAsync(appSetting);
            return new AppSettingDto(createdSetting);
        }catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<AppSettingDto> UpdateAppSetting(string id, AppSettingUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id cannot be null or empty", nameof(id));
        }
        try
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Setting cannot be null");
            }

            var existingSetting = await repository.SearchSettingsAsync(new AppSettingSearchOptions { Key = id });
            if (existingSetting == null)
            {
                throw new KeyNotFoundException($"Setting with key '{id}' not found.");
            }
            
            if(!string.IsNullOrEmpty(request.Value) && existingSetting.IsEncrypted)
            {
                request.Value = encryptionService.Encrypt(request.Value);
            }

            existingSetting.Key = request.Key ?? existingSetting.Key;
            existingSetting.Value = request.Value ?? existingSetting.Value;
            existingSetting.IsEncrypted = request.IsEncrypted ?? existingSetting.IsEncrypted;
            existingSetting.IsSystem = request.IsSystem ?? existingSetting.IsSystem;

            var updatedSetting = await repository.InsertOrUpdateSettingAsync(existingSetting);
            return new AppSettingDto(updatedSetting);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task DeleteSettingAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        }
        try
        {
            var setting = await repository.SearchSettingsAsync(new AppSettingSearchOptions { Key = key });
            if (setting == null)
            {
                throw new KeyNotFoundException($"Setting with key '{key}' not found.");
            }
            await repository.DeleteSettingAsync(setting.Id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<string> GetDecrypted(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key), "Setting cannot be null");
        }

        try
        {
            var setting = await repository.SearchSettingsAsync(new AppSettingSearchOptions { Key = key });
            if (setting == null)
            {
                throw new KeyNotFoundException($"Setting with key '{key}' not found.");
            }

            if (setting.IsEncrypted)
            {
                setting.Value = encryptionService.Decrypt(setting.Value);
            }

            return setting.Value;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task SetEncrypted(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        }
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Value cannot be null");
        }

        try
        {
            var setting = await repository.SearchSettingsAsync(new AppSettingSearchOptions { Key = key });
            if (setting == null)
            {
                throw new KeyNotFoundException($"Setting with key '{key}' not found.");
            }

            setting.Value = encryptionService.Encrypt(value);
            setting.IsEncrypted = true;

            await repository.InsertOrUpdateSettingAsync(setting);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}