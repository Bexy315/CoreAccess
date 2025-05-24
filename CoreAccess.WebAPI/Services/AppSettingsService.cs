using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Repositories;

namespace CoreAccess.WebAPI.Services;

public interface IAppSettingsService
{
    public Task<AppSettingDto?> GetSettingAsync(AppSettingSearchOptions options);
    public Task<AppSettingDto> CreateAppSetting(AppSettingCreateRequest request);
    public Task<AppSettingDto> UpdateAppSetting(string id, AppSettingUpdateRequest request);
    public Task DeleteSettingAsync(string key);
}

public class AppSettingsService(AppSettingsRepository repository, AesEncryptionService encryptionService) : IAppSettingsService
{
    public async Task<AppSettingDto?> GetSettingAsync(AppSettingSearchOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options), "Search options cannot be null");
        }
        try
        {
            var setting = await repository.GetSettingsAsync(options);
            if (setting == null)
            {
                return null;
            }

            return new AppSettingDto(setting);
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

            var existingSetting = await repository.GetSettingsAsync(new AppSettingSearchOptions { Key = id });
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
            var setting = await repository.GetSettingsAsync(new AppSettingSearchOptions { Key = key });
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
}