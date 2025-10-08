using CoreAccess.DataLayer.DbContext;
using CoreAccess.Models;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace CoreAccess.DataLayer.Repositories;

public interface IPermissionRepository
{
    public Task<PagedResult<Permission>> SearchPermissionsAsync(PermissionSearchOptions options, CancellationToken cancellationToken = default);
    public Task<Permission> InsertOrUpdatePermissionAsync(Permission permission, CancellationToken cancellationToken = default);
    public Task DeletePermissionAsync(string id, CancellationToken cancellationToken = default);
}
public class PermissionRepository(CoreAccessDbContext context) : IPermissionRepository
{
    public async Task<PagedResult<Permission>> SearchPermissionsAsync(PermissionSearchOptions options, CancellationToken cancellationToken = default)
    {
        var query = context.Permissions.AsQueryable();
        
        if(options.IncludeRoles??false)
            query = query.Include(p => p.Roles);
        
        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            query = query.Where(p => p.Name.Contains(options.Search) || (p.Description != null && p.Description.Contains(options.Search)));
        }
        
        if (!string.IsNullOrEmpty(options.Id))
        {
            query = query.Where(p => p.Id == options.Id);
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
        
        var totalCount = await query.CountAsync(cancellationToken);

        query = query.OrderBy(r => r.Name).ThenBy(r => r.Id);

        var items = await query
            .Skip((options.Page - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Permission>
        {
            Items = items,
            Page = options.Page,
            PageSize = options.PageSize,
            TotalCount = totalCount
        };
    }
    public async Task<Permission> InsertOrUpdatePermissionAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        var existingPermission = await context.Set<Permission>().FirstOrDefaultAsync(p => p.Id == permission.Id, cancellationToken);
        
        if (existingPermission == null)
        {
            var newPermission = await context.Set<Permission>().AddAsync(permission, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            
            return newPermission.Entity;
        }

        context.Entry(existingPermission).CurrentValues.SetValues(permission);
        context.Entry(existingPermission).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
        
        existingPermission = await context.Set<Permission>().FirstOrDefaultAsync(p => p.Id == permission.Id, cancellationToken);
        
        return existingPermission;
    }
    public async Task DeletePermissionAsync(string id, CancellationToken cancellationToken = default)
    {
        var permission = await context.Set<Permission>().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (permission != null)
        {
            context.Permissions.Remove(permission);
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            throw new KeyNotFoundException($"Permission with id {id} not found");
        }
    }
}