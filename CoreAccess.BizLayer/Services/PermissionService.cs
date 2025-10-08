using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;
using CoreAccess.Models.Extensions;

namespace CoreAccess.BizLayer.Services;

public interface IPermissionService
{
    Task<PagedResult<PermissionDto> > SearchPermissionsAsync(PermissionSearchOptions options, CancellationToken cancellationToken = default);
    Task<PermissionDetailDto> GetPermissionByIdAsync(string id, bool includeRoles = false, CancellationToken cancellationToken = default);
    Task<PermissionDetailDto> CreatePermissionAsync(PermissionCreateRequest request, CancellationToken cancellationToken = default);
    Task<PermissionDetailDto> UpdatePermissionAsync(string id, PermissionUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeletePermissionAsync(string id, CancellationToken cancellationToken = default);
    Task<List<PermissionDto>> GetPermissionsByRolesAsync(List<string> roles, CancellationToken cancellationToken = default);
}

public class PermissionService(IPermissionRepository permissionRepository) : IPermissionService
{
    public async Task<PagedResult<PermissionDto>> SearchPermissionsAsync(PermissionSearchOptions options, CancellationToken cancellationToken = default)
    {
        if (options == null)
        {
            options = new PermissionSearchOptions();
        }

        var result = await permissionRepository.SearchPermissionsAsync(options, cancellationToken);

        return new PagedResult<PermissionDto>
        {
            Items = result.Items.Select(x => x.ToDto()).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<PermissionDetailDto> GetPermissionByIdAsync(string id,bool includeRoles = false, CancellationToken cancellationToken = default)
    {
        var permission = await permissionRepository.SearchPermissionsAsync(new PermissionSearchOptions(){Id = id, IncludeRoles = includeRoles}, cancellationToken);
        if (permission == null || !permission.Items.Any())
        {
            return null;
        }

        return permission.Items.FirstOrDefault().ToDetailDto();
    }

    public async Task<PermissionDetailDto> CreatePermissionAsync(PermissionCreateRequest request, CancellationToken cancellationToken = default)
    {
        var newPermission = new Permission
        {
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
        };

        var createdPermission = await permissionRepository.InsertOrUpdatePermissionAsync(newPermission, cancellationToken);
        if (createdPermission == null)
        {
            throw new Exception("Failed to create permission");
        }
        return createdPermission.ToDetailDto();
    }

    public async Task<PermissionDetailDto> UpdatePermissionAsync(string id, PermissionUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var existingPermission = await permissionRepository.SearchPermissionsAsync(new PermissionSearchOptions() { Id = id }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken);
        if (existingPermission == null)
        {
            throw new Exception("Permission not found");
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            existingPermission.Name = request.Name;
        }
        if (request.Description != null)
        {
            existingPermission.Description = request.Description;
        }
        existingPermission.UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        var updatedPermission = await permissionRepository.InsertOrUpdatePermissionAsync(existingPermission, cancellationToken);
        if (updatedPermission == null)
        {
            throw new Exception("Failed to update permission");
        }
        return updatedPermission.ToDetailDto();
    }

    public async Task DeletePermissionAsync(string id, CancellationToken cancellationToken = default)
    {
        await permissionRepository.DeletePermissionAsync(id, cancellationToken);
    }

    public async Task<List<PermissionDto>> GetPermissionsByRolesAsync(List<string> roles, CancellationToken cancellationToken = default)
    {
        if (roles == null || roles.Count == 0)
        {
            return new List<PermissionDto>();
        }

        var permissions = await permissionRepository.SearchPermissionsAsync(new PermissionSearchOptions(){Roles = roles}, cancellationToken: cancellationToken);
        
        return permissions.Items.Select(x => x.ToDto()).ToList();
    }
}