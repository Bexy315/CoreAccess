using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;

namespace CoreAccess.BizLayer.Services;

public interface IPermissionService
{
    Task<List<Permission>> GetPermissionsByRolesAsync(List<string> roles, CancellationToken cancellationToken = default);
}

public class PermissionService(IPermissionRepository permissionRepository) : IPermissionService
{
    public async Task<List<Permission>> GetPermissionsByRolesAsync(List<string> roles, CancellationToken cancellationToken = default)
    {
        if (roles == null || roles.Count == 0)
        {
            return new List<Permission>();
        }

        var permissions = await permissionRepository.SearchPermissionsAsync(roleFilters: roles, cancellationToken: cancellationToken);
        
        return permissions;
    }
}