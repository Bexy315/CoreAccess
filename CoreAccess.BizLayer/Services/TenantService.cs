using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;

namespace CoreAccess.BizLayer.Services;

public interface ITenantService
{
    Task<TenantDto> CreateTenantAsync(TenantCreateRequest request, CancellationToken cancellationToken = default);
}

public class TenantService(ITenantRepository tenantRepository) : ITenantService
{
    public async Task<TenantDto> CreateTenantAsync(TenantCreateRequest request, CancellationToken cancellationToken = default)
    {
        var tenant = new Tenant
        {
            Slug = request.Slug,
            DisplayName = request.DisplayName,
            Description = request.Description,
            LogoUrl = request.LogoUrl,
            IsActive = request.IsActive
        };

        var createdTenant = await tenantRepository.InsertOrUpdateUserAsync(tenant, cancellationToken);
        
        return new TenantDto
        {
            Id = createdTenant.Id,
            Slug = createdTenant.Slug,
            DisplayName = createdTenant.DisplayName,
            Description = createdTenant.Description,
            LogoUrl = createdTenant.LogoUrl,
            CreatedAt = createdTenant.CreatedAt,
            UpdatedAt = createdTenant.UpdatedAt,
            IsActive = createdTenant.IsActive
        };
    }
}