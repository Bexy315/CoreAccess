using CoreAccess.DataLayer.DbContext;
using CoreAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.DataLayer.Repositories;

public interface IPermissionRepository
{
    Task<List<Permission>> SearchPermissionsAsync(PermissionSearchOptions options, CancellationToken cancellationToken = default);
    Task<List<Permission>> GetAllPermissionsAsync();
    Task<Permission> GetPermissionByNameAsync(string permissionName);
    Task<Permission?> GetPermissionByIdAsync(Guid id);
    Task<Permission> CreatePermissionAsync(CreatePermissionRequest request);
    Task DeletePermissionAsync(Guid id);
}
public class PermissionRepository(CoreAccessDbContext context) : IPermissionRepository
{
    public async Task<List<Permission>> SearchPermissionsAsync(PermissionSearchOptions options, CancellationToken cancellationToken = default)
    {
        var query = context.Permissions.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            query = query.Where(p => p.Name.Contains(options.Search) || (p.Description != null && p.Description.Contains(options.Search)));
        }
        
        if (!string.IsNullOrWhiteSpace(options.Name))
        {
            query = query.Where(p => p.Name.Contains(options.Name));
        }
        
        if (options.Roles != null && options.Roles.Count > 0)
        {
            query = query.Where(p => p.Roles.Any(r => options.Roles.Contains(r.Name)));
        }

        if (!string.IsNullOrWhiteSpace(options.Description))
        {
            query = query.Where(p => p.Description != null && p.Description.Contains(options.Description));
        }
        
        if (options.IsSystem.HasValue)
        {
            query = query.Where(p => p.IsSystem == options.IsSystem.Value);
        }
        
        return await query
            .Skip((options.Page - 1) * options.PageSize)
            .Take(options.PageSize)
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