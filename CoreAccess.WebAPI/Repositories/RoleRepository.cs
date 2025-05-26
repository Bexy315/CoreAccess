using CoreAccess.WebAPI.DbContext;
using CoreAccess.WebAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.WebAPI.Repositories;

public interface IRoleRepository
{
    public Task<List<CoreRole>> SearchRolesAsync(CoreRoleSearchOptions options, CancellationToken cancellationToken = default);
    public Task<CoreRole> InsertOrUpdateRoleAsync(CoreRole user, CancellationToken cancellationToken = default);
    public Task DeleteRoleAsync(string id, CancellationToken cancellationToken = default);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class RoleRepository(CoreAccessDbContext context) : IRoleRepository
{
    public async Task<List<CoreRole>> SearchRolesAsync(CoreRoleSearchOptions options, CancellationToken cancellationToken = default)
    {
        var users = await context.Roles.ToListAsync(cancellationToken);
        
        var query = users.AsQueryable();

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
        var result = query.Skip(skip).Take(options.PageSize).ToList();
        
        return result;
    }

    public async Task<CoreRole> InsertOrUpdateRoleAsync(CoreRole role, CancellationToken cancellationToken = default)
    {
        var existingRole = await context.Set<CoreRole>().FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);
        
        if (existingRole == null)
        {
            context.Set<CoreRole>().Add(role);
        }
        else
        {
            context.Entry(existingRole).CurrentValues.SetValues(role);
        }

        return role;
    }

    public async Task DeleteRoleAsync(string id, CancellationToken cancellationToken = default)
    {
        var role = await context.Roles.FindAsync(id, cancellationToken);
        if (role != null)
        {
            context.Roles.Remove(role);
        }
        else
        {
            throw new Exception("Role not found");
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}