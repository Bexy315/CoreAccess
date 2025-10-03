using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreAccess.DataLayer.DbContext;
using CoreAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.DataLayer.Repositories;

public interface IRoleRepository
{
    public Task<List<Role>> SearchRolesAsync(RoleSearchOptions options, CancellationToken cancellationToken = default);
    public Task<Role> InsertOrUpdateRoleAsync(Role user, CancellationToken cancellationToken = default);
    public Task DeleteRoleAsync(string id, CancellationToken cancellationToken = default);
}
public class RoleRepository(CoreAccessDbContext context) : IRoleRepository
{
    public async Task<List<Role>> SearchRolesAsync(RoleSearchOptions options, CancellationToken cancellationToken = default)
    {
        if (options == null) options = new RoleSearchOptions();

        var page = Math.Max(1, options.Page);
        var pageSize = options.PageSize <= 0 ? 50 : options.PageSize;
        var skip = (page - 1) * pageSize;

        IQueryable<Role> query = context.Roles.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            var search = options.Search.Trim().ToLowerInvariant();
            var like = $"%{search}%";

            query = query.Where(r =>
                EF.Functions.Like(r.Name.ToLower(), like) ||
                (r.Description != null && EF.Functions.Like(r.Description.ToLower(), like))
            );
        }
        
        if (!string.IsNullOrWhiteSpace(options.Id))
        {
            query = query.Where(r => r.Id == options.Id);
        }

        if (!string.IsNullOrWhiteSpace(options.Name))
        {
            var name = options.Name.Trim().ToLowerInvariant();
            var likeName = $"%{name}%";
            query = query.Where(r => EF.Functions.Like(r.Name.ToLower(), likeName));
        }

        if (!string.IsNullOrWhiteSpace(options.Description))
        {
            var desc = options.Description.Trim().ToLowerInvariant();
            var likeDesc = $"%{desc}%";
            query = query.Where(r => r.Description != null && EF.Functions.Like(r.Description.ToLower(), likeDesc));
        }

        if (options.IsSystem.HasValue)
        {
            query = query.Where(r => r.IsSystem == options.IsSystem.Value);
        }
        
        if(options.IncludeUsers.HasValue && options.IncludeUsers.Value)
        {
            query = query.Include(r => r.Users);
        }
        
        if(options.IncludePermissions.HasValue && options.IncludePermissions.Value)
        {
            query = query.Include(r => r.Permissions);
        }

        query = query.OrderBy(r => r.Name).ThenBy(r => r.Id);

        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return items;
    }
    public async Task<Role> InsertOrUpdateRoleAsync(Role role, CancellationToken cancellationToken = default)
    {
        var existingRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);
        
        if (existingRole == null)
        {
            var newRole = await context.Set<Role>().AddAsync(role, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            
            return newRole.Entity;
        }
        
        role.UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        context.Entry(existingRole).CurrentValues.SetValues(role);
        context.Entry(existingRole).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
        
        existingRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);
        
        return existingRole;
    }
    public async Task DeleteRoleAsync(string id, CancellationToken cancellationToken = default)
    {
        var role = await context.Set<Role>().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (role != null)
        {
            context.Roles.Remove(role);
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            throw new KeyNotFoundException($"Role with id {id} not found");
        }
    }
}