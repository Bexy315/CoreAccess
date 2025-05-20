using CoreAccess.WebAPI.DbContext;
using CoreAccess.WebAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.WebAPI.Repositories;

public interface IRoleRepository
{
    public Task<List<CoreRole>> SearchRolesAsync(CoreRoleSearchOptions options);
    public Task<CoreRole> InsertOrUpdateRoleAsync(CoreRole user);
    public Task DeleteRoleAsync(string id); 
}

public class RoleRepository(CoreAccessDbContext context) : IRoleRepository
{
    public async Task<List<CoreRole>> SearchRolesAsync(CoreRoleSearchOptions options)
    {
        var query = context.Roles.AsQueryable();
        
        if (!string.IsNullOrEmpty(options.Search))
        {
            query = query.Where(r => r.Name.Contains(options.Search) ||
                                     (r.Description != null && r.Description.Contains(options.Search)));
        }
        
        if (!string.IsNullOrEmpty(options.Id))
        {
            query = query.Where(r => r.Id.ToString() == options.Id);
        }
        
        if (!string.IsNullOrEmpty(options.Name))
        {
            query = query.Where(r => r.Name.Contains(options.Name));
        }
        
        if (!string.IsNullOrEmpty(options.Description))
        {
            query = query.Where(r => r.Description != null && r.Description.Contains(options.Description));
        }
        
        if (options.IsSystem.HasValue)
        {
            query = query.Where(r => r.IsSystem == options.IsSystem.Value);
        }
        
        return await query.ToListAsync();
    }

    public async Task<CoreRole> InsertOrUpdateRoleAsync(CoreRole user)
    {
        var existingRole = await context.Roles.FindAsync(user.Id);
        if (existingRole != null)
        {
            context.Entry(existingRole).CurrentValues.SetValues(user);
        }
        else
        {
            await context.Roles.AddAsync(user);
        }

        await context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteRoleAsync(string id)
    {
        var role = await context.Roles.FindAsync(id);
        if (role != null)
        {
            context.Roles.Remove(role);
            await context.SaveChangesAsync();
        }
        else
        {
            throw new Exception("Role not found");
        }
    }
}