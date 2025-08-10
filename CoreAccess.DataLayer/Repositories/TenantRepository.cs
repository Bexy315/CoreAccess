using CoreAccess.DataLayer.DbContext;
using CoreAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.DataLayer.Repositories;


public interface ITenantRepository
{
    public Task<List<Tenant>> SearchTenantsAsync(TenantSearchOptions options, CancellationToken cancellationToken = default);
    public Task<Tenant> InsertOrUpdateUserAsync(Tenant tenant, CancellationToken cancellationToken = default);
    public Task DeleteTenantAsync(string tenantId, CancellationToken cancellationToken = default);
}

public class TenantRepository(CoreAccessDbContext context) : ITenantRepository
{
    public async Task<List<Tenant>> SearchTenantsAsync(TenantSearchOptions options, CancellationToken cancellationToken = default)
    {
        var tenants = await context.Tenants.Include(t => t.Users)
                                            .Include(t => t.Roles)
                                            .Include(t => t.Permissions)
                                            .ToListAsync(cancellationToken);

        var query = tenants.AsQueryable();

        if (!string.IsNullOrEmpty(options.Search))
        {
            query = query.Where(t => t.DisplayName.ToLower().Contains(options.Search.ToLower()) ||
                                     t.Description.ToLower().Contains(options.Search.ToLower()) ||
                                     t.Slug.ToLower().Contains(options.Search.ToLower()));
        }

        if (!string.IsNullOrEmpty(options.Id))
        {
            var idLower = options.Id.ToLower();
            query = query.Where(t => t.Id.ToString().ToLower() == idLower);
        }

        if (!string.IsNullOrEmpty(options.Slug))
        {
            query = query.Where(t => t.Slug.ToLower().Contains(options.Slug.ToLower()));
        }

        if (!string.IsNullOrEmpty(options.DisplayName))
        {
            query = query.Where(t => t.DisplayName.ToLower().Contains(options.DisplayName.ToLower()));
        }

        if (!string.IsNullOrEmpty(options.Description))
        {
            query = query.Where(t => t.Description.ToLower().Contains(options.Description.ToLower()));
        }

        if (options.IsActive.HasValue)
        {
            query = query.Where(t => t.IsActive == options.IsActive.Value);
        }

        return await query.Skip((options.Page - 1) * options.PageSize)
                          .Take(options.PageSize)
                          .ToListAsync(cancellationToken);
    }

    public async Task<Tenant> InsertOrUpdateUserAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        if (tenant.Id == Guid.Empty)
        {
            tenant.Id = Guid.NewGuid();
            context.Tenants.Add(tenant);
        }
        else
        {
            var existingTenant = await context.Tenants.FindAsync(new object[] { tenant.Id }, cancellationToken);
            if (existingTenant != null)
            {
                context.Entry(existingTenant).CurrentValues.SetValues(tenant);
            }
            else
            {
                context.Tenants.Add(tenant);
            }
        }
        
        await context.SaveChangesAsync(cancellationToken);
        
        return tenant;
    }

    public async Task DeleteTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new ArgumentException("Tenant ID cannot be null or empty.", nameof(tenantId));
        }

        var tenant = await context.Tenants.FindAsync(Guid.Parse(tenantId), cancellationToken);
        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");
        }

        context.Tenants.Remove(tenant);
        await context.SaveChangesAsync(cancellationToken);
    }
}