using CoreAccess.DataLayer.DbContext;
using CoreAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.DataLayer.Repositories;

public interface ISettingsRepository
{
    Task<List<Setting>> SearchSettingsAsync(SettingSearchOptions options, CancellationToken cancellationToken = default);
    public Task<Setting> InsertOrUpdateSettingAsync(Setting user, CancellationToken cancellationToken = default);
    public Task DeleteSettingAsync(string id, CancellationToken cancellationToken = default);
}

public class SettingsRepository(CoreAccessDbContext context) : ISettingsRepository
{
    public async Task<List<Setting>> SearchSettingsAsync(SettingSearchOptions options, CancellationToken cancellationToken = default)
    {
        var query = context.Settings.AsQueryable();

        if (!string.IsNullOrEmpty(options.Key))
        {
            query = query.Where(s => s.Key.Contains(options.Key));
        }

        if (options.IsSecret.HasValue)
        {
            query = query.Where(s => s.IsSecret == options.IsSecret.Value);
        }

        query = query
            .Skip((options.Page - 1) * options.PageSize)
            .Take(options.PageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Setting> InsertOrUpdateSettingAsync(Setting setting, CancellationToken cancellationToken = default)
    {
        var existingSetting = await context.Set<Setting>().FirstOrDefaultAsync(s => s.Id == setting.Id, cancellationToken);
        
        if (existingSetting == null)
        {
            var newRole = await context.Set<Setting>().AddAsync(setting, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            
            return newRole.Entity;
        }

        context.Entry(existingSetting).CurrentValues.SetValues(setting);
        context.Entry(existingSetting).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
        
        existingSetting = await context.Set<Setting>().FirstOrDefaultAsync(s => s.Id == setting.Id, cancellationToken);
        
        return existingSetting;
    }

    public async Task DeleteSettingAsync(string id, CancellationToken cancellationToken = default)
    {
        var setting = await context.Settings.FindAsync(id, cancellationToken);
        if (setting != null)
        {
            context.Settings.Remove(setting);
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            throw new Exception("Role not found");
        }
    }
}