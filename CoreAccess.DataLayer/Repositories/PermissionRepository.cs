using CoreAccess.DataLayer.DbContext;
using CoreAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.DataLayer.Repositories;

public interface IPermissionRepository
{
    Task<List<Permission>> SearchPermissionsAsync(string? nameFilter = null, List<string>? roleFilters = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<List<Permission>> GetAllPermissionsAsync();
    Task<Permission> GetPermissionByNameAsync(string permissionName);
    Task<Permission?> GetPermissionByIdAsync(Guid id);
    Task<Permission> CreatePermissionAsync(CreatePermissionRequest request);
    Task DeletePermissionAsync(Guid id);
}
public class PermissionRepository(CoreAccessDbContext context) : IPermissionRepository
{
    public async Task<List<Permission>> SearchPermissionsAsync(string? nameFilter = null, List<string>? roleFilters = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = context.Permissions.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(p => p.Name.Contains(nameFilter));
        }
        
        if (roleFilters != null && roleFilters.Count > 0)
        {
            query = query.Where(p => p.Roles.Any(r => roleFilters.Contains(r.Name)));
        }
        
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken: cancellationToken);
    }

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

    public async Task<Permission> CreatePermissionAsync(CreatePermissionRequest request)
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
        await context.SaveChangesAsync();
        return newPermission;
    }

    public async Task DeletePermissionAsync(Guid id)
    {
        var permission = await GetPermissionByIdAsync(id);
        if (permission != null)
        {
            context.Permissions.Remove(permission);
            await context.SaveChangesAsync();
        }
    }
}