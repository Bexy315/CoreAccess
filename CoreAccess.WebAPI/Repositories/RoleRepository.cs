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
            query = query.Where(r => r.Name.ToLower().Contains(options.Search.ToLower()) ||
                                     (r.Description != null && r.Description.ToLower().Contains(options.Search.ToLower())));
        }

        if (!string.IsNullOrEmpty(options.Id))
        {
            var idLower = options.Id.ToLower();
            query = query.Where(r => r.Id.ToString().ToLower() == idLower);
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
        var result = await query.Skip(skip).Take(options.PageSize).ToListAsync();

        return result;
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