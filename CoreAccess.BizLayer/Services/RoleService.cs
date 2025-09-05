using CoreAccess.BizLayer.Logger;
using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;
using CoreAccess.Models.Extensions;
using CoreAccess.WebAPI.Logger;
using CoreAccess.WebAPI.Logger.Model;

namespace CoreAccess.BizLayer.Services;

public interface IRoleService
{
    Task<PagedResult<RoleDto>> SearchRolesAsync(RoleSearchOptions options, CancellationToken cancellationToken = default);
    Task<RoleDetailDto> GetRoleByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<RoleDetailDto> CreateRoleAsync(RoleCreateRequest request, CancellationToken cancellationToken = default);
    Task<RoleDetailDto> UpdateRoleAsync(string userId, RoleUpdateRequest user, CancellationToken cancellationToken = default);
    Task AddPermissionToRoleAsync(string roleName, string permissionName, CancellationToken cancellationToken = default);
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
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(RoleService), "Error searching Roles", ex);
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
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(RoleService), "Error getting Role by ID", ex);
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
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(RoleService), "Error creating Role", ex);
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
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(RoleService), "Error updating Role", ex);
            throw;
        }
    }

    public async Task AddPermissionToRoleAsync(string roleName, string permissionName, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await roleRepository.SearchRolesAsync(new RoleSearchOptions()
            {
                Name = roleName,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.FirstOrDefault() ?? null, cancellationToken);
            
            if (role == null)
            {
                throw new Exception($"Role '{roleName}' not found");
            }

            var permission = await permissionRepository.SearchPermissionsAsync(new PermissionSearchOptions(){Name = permissionName}, cancellationToken).ContinueWith(x => x.Result.FirstOrDefault() ?? null, cancellationToken);
            
            if (permission == null)
            {
                throw new Exception($"Permission '{permissionName}' not found");
            }

            if (role.Permissions.All(p => p.Name != permission.Name))
            {
                role.Permissions.Add(permission);
            }
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(RoleService), "Error adding Permission to Role",ex);
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
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(RoleService), "Error deleting Role", ex);
            throw;
        }
    }
}