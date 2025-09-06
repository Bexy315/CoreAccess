using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;
using CoreAccess.Models.Extensions;

namespace CoreAccess.BizLayer.Services;

public interface IRoleService
{
    Task<PagedResult<RoleDto>> SearchRolesAsync(RoleSearchOptions options, CancellationToken cancellationToken = default);
    Task<RoleDetailDto> GetRoleByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<RoleDetailDto> CreateRoleAsync(RoleCreateRequest request, CancellationToken cancellationToken = default);
    Task<RoleDetailDto> UpdateRoleAsync(string userId, RoleUpdateRequest user, CancellationToken cancellationToken = default);
    Task<RoleDetailDto> AddPermissionToRoleAsync(string roleId, string permissionId, CancellationToken cancellationToken = default);
    Task DeleteRoleAsync(string id, CancellationToken cancellationToken = default);
}

public class RoleService(IRoleRepository roleRepository, IPermissionRepository permissionRepository) : IRoleService
{
    public async Task<PagedResult<RoleDto>> SearchRolesAsync(RoleSearchOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await roleRepository.SearchRolesAsync(options, cancellationToken);
            var dto = new PagedResult<RoleDto>
            {
                Items = result.Select(x => x.ToDto()).ToList(),
                TotalCount = result.Count,
                Page = options.Page,
                PageSize = options.PageSize
            };
            return dto;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<RoleDetailDto> GetRoleByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await roleRepository.SearchRolesAsync(new RoleSearchOptions() { Id = id }, cancellationToken).ContinueWith(t => t.Result.FirstOrDefault() ?? null, cancellationToken);
            if (role == null)
            {
                return null;
            }

            return role.ToDetailDto();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<RoleDetailDto> CreateRoleAsync(RoleCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var newRole = new Role
            {
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            };

            var createdRole = await roleRepository.InsertOrUpdateRoleAsync(newRole, cancellationToken);
            if (createdRole == null)
            {
                throw new Exception("Failed to create role");
            }
            return createdRole.ToDetailDto();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<RoleDetailDto> UpdateRoleAsync(string userId, RoleUpdateRequest user, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingRole = await roleRepository.SearchRolesAsync(new RoleSearchOptions()
            {
                Id = userId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.FirstOrDefault() ?? null, cancellationToken);
            
            if (existingRole == null)
            {
                throw new Exception("Role not found");
            }
            
            existingRole.Name = user.Name ?? existingRole.Name;
            existingRole.Description = user.Description ?? existingRole.Description;
            existingRole.UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var role = await roleRepository.InsertOrUpdateRoleAsync(existingRole, cancellationToken);
            if (role == null)
            {
                throw new Exception("Failed to update role");
            }
            return role.ToDetailDto();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<RoleDetailDto> AddPermissionToRoleAsync(string roleId, string permissionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await roleRepository.SearchRolesAsync(new RoleSearchOptions()
            {
                Id = roleId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.FirstOrDefault() ?? null, cancellationToken);
            
            if (role == null)
            {
                throw new Exception($"Role '{roleId}' not found");
            }

            var permission = await permissionRepository.SearchPermissionsAsync(new PermissionSearchOptions()
            {
                Id = permissionId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(x => x.Result.FirstOrDefault() ?? null, cancellationToken);
            
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
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task DeleteRoleAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            await roleRepository.DeleteRoleAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}