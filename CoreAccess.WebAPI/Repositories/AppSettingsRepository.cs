using CoreAccess.WebAPI.DbContext;
using CoreAccess.WebAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.WebAPI.Repositories;

public interface IAppSettingsRepository
{
    public Task<AppSetting?> SearchSettingsAsync(AppSettingSearchOptions options);
    public Task<AppSetting> InsertOrUpdateSettingAsync(AppSetting setting);
    public Task DeleteSettingAsync(Guid id);
}

public class AppSettingsRepository(CoreAccessDbContext context) : IAppSettingsRepository
{
    public async Task<AppSetting?> SearchSettingsAsync(AppSettingSearchOptions options)
    {
        var query = context.AppSettings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(options.Key))
        {
            query = query.Where(s => s.Key == options.Key);
        }

        if (!string.IsNullOrWhiteSpace(options.Value))
        {
            query = query.Where(s => s.Value == options.Value);
        }

        if (!options.IncludeSystemSettings)
        {
            query = query.Where(s => !s.IsSystem);
        }

        if (!options.IncludeEncryptedSettings)
        {
            query = query.Where(s => !s.IsEncrypted);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<AppSetting> InsertOrUpdateSettingAsync(AppSetting setting)
    {
        var existingSetting = await context.Set<AppSetting>().FirstOrDefaultAsync(s => s.Id == setting.Id);
        if (existingSetting == null)
        {
            context.Set<AppSetting>().Add(setting);
        }
        else
        {
            context.Entry(existingSetting).CurrentValues.SetValues(setting);
        }
        
        await context.SaveChangesAsync();
        return setting;
    }

    public async Task DeleteSettingAsync(Guid id)
    {
        var setting = await context.AppSettings.FindAsync(id);
        if (setting != null)
        {
            context.AppSettings.Remove(setting);
            await context.SaveChangesAsync();
        }
        else
        {
            throw new KeyNotFoundException($"Setting with ID {id} not found.");
        }
    }
}