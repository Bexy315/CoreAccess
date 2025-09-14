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
        var users = await context.Roles.ToListAsync(cancellationToken);
        
        var query = users.AsQueryable();

        if (!string.IsNullOrEmpty(options.Search))
        {
            query = query.Where(r => r.Name.ToLower().Contains(options.Search.ToLower()) ||
                                     (r.Description != null && r.Description.ToLower().Contains(options.Search.ToLower())));
        }

        if (!string.IsNullOrEmpty(options.Id) && Guid.TryParse(options.Id, out var guidId))
        {
            query = query.Where(p => p.Id == guidId);
        }

        if (!string.IsNullOrEmpty(options.Name))
        {
            query = query.Where(r => r.Name.ToLower().Contains(options.Name.ToLower()));
        }

        if (!string.IsNullOrEmpty(options.Description))
        {
            query = query.Where(r => r.Description != null && r.Description.ToLower().Contains(options.Description.ToLower()));
        }

        if (options.IsSystem.HasValue)
        {
            query = query.Where(r => r.IsSystem == options.IsSystem.Value);
        }

        var skip = (options.Page - 1) * options.PageSize;
        var result = query.Skip(skip).Take(options.PageSize).ToList();
        
        return result;
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

        context.Entry(existingRole).CurrentValues.SetValues(role);
        context.Entry(existingRole).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
        
        existingRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);
        
        return existingRole;
    }
    public async Task DeleteRoleAsync(string id, CancellationToken cancellationToken = default)
    {
        var role = await context.Set<Role>().FirstOrDefaultAsync(r => r.Id == Guid.Parse(id), cancellationToken);
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