using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;
using CoreAccess.Models.Extensions;

namespace CoreAccess.BizLayer.Services;

public interface IRoleService
{
    Task<PagedResult<RoleDto>> SearchRolesAsync(RoleSearchOptions options, CancellationToken cancellationToken = default);
    Task<RoleDetailDto> GetRoleByIdAsync(string id, bool includeUsers = false, bool includePermissions = false, CancellationToken cancellationToken = default);
    Task<RoleDetailDto> CreateRoleAsync(RoleCreateRequest request, bool isSystem = false, CancellationToken cancellationToken = default);
    Task<RoleDetailDto> UpdateRoleAsync(string userId, RoleUpdateRequest user, CancellationToken cancellationToken = default);
    Task<RoleDetailDto> AddPermissionToRoleAsync(string roleId, string permissionId, CancellationToken cancellationToken = default);
    Task<RoleDetailDto> RemovePermissionFromRoleAsync(string roleId, string permissionId, CancellationToken cancellationToken = default);
    Task DeleteRoleAsync(string id, CancellationToken cancellationToken = default);
}

public class RoleService(IRoleRepository roleRepository, IPermissionRepository permissionRepository) : IRoleService
{
    public async Task<PagedResult<RoleDto>> SearchRolesAsync(RoleSearchOptions options, CancellationToken cancellationToken = default)
    {
            var result = await roleRepository.SearchRolesAsync(options, cancellationToken);
            var dto = new PagedResult<RoleDto>
            {
                Items = result.Items.Select(x => x.ToDto()).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
            return dto;
    }

    public async Task<RoleDetailDto> GetRoleByIdAsync(string id, bool includeUsers = false, bool includePermissions = false, CancellationToken cancellationToken = default)
    {
            var role = await roleRepository.SearchRolesAsync(new RoleSearchOptions() { Id = id, IncludeUsers = includeUsers, IncludePermissions = includePermissions}, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken);
            if (role == null)
            {
                return null;
            }

            return role.ToDetailDto();
    }

    public async Task<RoleDetailDto> CreateRoleAsync(RoleCreateRequest request, bool isSystem = false,  CancellationToken cancellationToken = default)
    {
            var newRole = new Role
            {
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                IsSystem = isSystem
            };

            var createdRole = await roleRepository.InsertOrUpdateRoleAsync(newRole, cancellationToken);
            if (createdRole == null)
            {
                throw new Exception("Failed to create role");
            }
            return createdRole.ToDetailDto();
    }

    public async Task<RoleDetailDto> UpdateRoleAsync(string userId, RoleUpdateRequest request, CancellationToken cancellationToken = default)
    {
            var existingRole = await roleRepository.SearchRolesAsync(new RoleSearchOptions()
            {
                Id = userId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken);
            
            if (existingRole == null)
            {
                throw new Exception("Role not found");
            }
            
            existingRole.Name = request.Name ?? existingRole.Name;
            existingRole.Description = request.Description ?? existingRole.Description;

            var role = await roleRepository.InsertOrUpdateRoleAsync(existingRole, cancellationToken);
            if (role == null)
            {
                throw new Exception("Failed to update role");
            }
            return role.ToDetailDto();
    }

    public async Task<RoleDetailDto> AddPermissionToRoleAsync(string roleId, string permissionId, CancellationToken cancellationToken = default)
    {
            var role = await roleRepository.SearchRolesAsync(new RoleSearchOptions()
            {
                Id = roleId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken);
            
            if (role == null)
            {
                throw new Exception($"Role '{roleId}' not found");
            }

            var permission = await permissionRepository.SearchPermissionsAsync(new PermissionSearchOptions()
            {
                Id = permissionId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(x => x.Result.Items.FirstOrDefault() ?? null, cancellationToken);
            
            if (permission == null)
            {
                throw new Exception($"Permission '{permissionId}' not found");
            }

            if (role.Permissions.Count != 0 && role.Permissions.Contains(permission))
            {
                throw new Exception($"Role {role.Name} already has permission {permission.Name}");
            }
            
            role.Permissions.Add(permission);
            var updatedRole = await roleRepository.InsertOrUpdateRoleAsync(role, cancellationToken);

            return updatedRole.ToDetailDto();
    }

    public async Task<RoleDetailDto> RemovePermissionFromRoleAsync(string roleId, string permissionId, CancellationToken cancellationToken = default)
    {
            var role = await roleRepository.SearchRolesAsync(new RoleSearchOptions()
            {
                Id = roleId,
                IncludePermissions = true,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken);
            
            if (role == null)
            {
                throw new Exception($"Role '{roleId}' not found");
            }

            var permission = await permissionRepository.SearchPermissionsAsync(new PermissionSearchOptions()
            {
                Id = permissionId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(x => x.Result.Items.FirstOrDefault() ?? null, cancellationToken);
            
            if (permission == null)
            {
                throw new Exception($"Permission '{permissionId}' not found");
            }

            if (role.Permissions.Count == 0 || !role.Permissions.Contains(permission))
            {
                throw new Exception($"Role {role.Name} does not have permission {permission.Name}");
            }
            
            role.Permissions.Remove(permission);
            var updatedRole = await roleRepository.InsertOrUpdateRoleAsync(role, cancellationToken);

            return updatedRole.ToDetailDto();
    }

    public async Task DeleteRoleAsync(string id, CancellationToken cancellationToken = default)
    {
            var role = await roleRepository.SearchRolesAsync(new RoleSearchOptions()
            {
                Id = id,
                IncludeUsers = true,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken);
            
            if(role == null)
                throw new Exception("Role not found");
            
            if(role.Users != null && role.Users.Count > 0)
                throw new Exception($"Cannot delete role with assigned users. Please remove all {role.Users.Count} users.");
            
            if(role.IsSystem)
                throw new Exception("Cannot delete system role.");
            
            await roleRepository.DeleteRoleAsync(id, cancellationToken);
    }
}