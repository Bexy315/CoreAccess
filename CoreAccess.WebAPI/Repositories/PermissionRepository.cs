using CoreAccess.WebAPI.DbContext;
using CoreAccess.WebAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.WebAPI.Repositories;

public interface IPermissionRepository
{
    Task<List<Permission>> GetAllPermissionsAsync();
    Task<Permission> GetPermissionByNameAsync(string permissionName);
    Task<Permission?> GetPermissionByIdAsync(Guid id);
    Task<Permission> AddPermissionAsync(CreatePermissionRequest request);
    Task DeletePermissionAsync(Guid id);
    Task SaveChangesAsync();
}
public class PermissionRepository(CoreAccessDbContext context) : IPermissionRepository
{

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        return await context.Permissions.ToListAsync();
    }

    public async Task<Permission> GetPermissionByNameAsync(string permissionName)
    {
        return await context.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName)
               ?? throw new KeyNotFoundException($"Permission '{permissionName}' not found.");
    }

    public async Task<Permission?> GetPermissionByIdAsync(Guid id)
    {
        return await context.Permissions.FindAsync(id);
    }

    public async Task<Permission> AddPermissionAsync(CreatePermissionRequest request)
    {
        var newPermission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsSystem = request.IsSystem,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        context.Permissions.Add(newPermission);
        await SaveChangesAsync();
        return newPermission;
    }

    public async Task DeletePermissionAsync(Guid id)
    {
        var permission = await GetPermissionByIdAsync(id);
        if (permission != null)
        {
            context.Permissions.Remove(permission);
            await SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}