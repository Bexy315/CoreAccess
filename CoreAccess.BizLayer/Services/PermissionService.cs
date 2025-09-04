using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;

namespace CoreAccess.BizLayer.Services;

public interface IPermissionService
{
    Task<PagedResult<PermissionDto> > SearchPermissionsAsync(PermissionSearchOptions options, CancellationToken cancellationToken = default);
    Task<List<Permission>> GetPermissionsByRolesAsync(List<string> roles, CancellationToken cancellationToken = default);
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
            Items = result.Select(x => new PermissionDto(x)).ToList(),
            TotalCount = result.Count,
            Page = options.Page,
            PageSize = options.PageSize
        };
    }

    public async Task<List<Permission>> GetPermissionsByRolesAsync(List<string> roles, CancellationToken cancellationToken = default)
    {
        if (roles == null || roles.Count == 0)
        {
            return new List<Permission>();
        }

        var permissions = await permissionRepository.SearchPermissionsAsync(new PermissionSearchOptions(){Roles = roles}, cancellationToken: cancellationToken);
        
        return permissions;
    }
}