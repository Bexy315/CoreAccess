using CoreAccess.BizLayer.Logger;
using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;
using CoreAccess.WebAPI.Logger;
using CoreAccess.WebAPI.Logger.Model;

namespace CoreAccess.BizLayer.Services;

public interface IRoleService
{
    Task<PagedResult<RoleDto>> SearchRolesAsync(RoleSearchOptions options);
    Task<RoleDto> CreateRoleAsync(RoleCreateRequest request);
    Task<RoleDto> UpdateRoleAsync(string userId, RoleUpdateRequest user);
    Task AddPermissionToRoleAsync(string roleName, string permissionName);
    Task DeleteRoleAsync(string id);
}

public class RoleService(IRoleRepository roleRepository, IPermissionRepository permissionRepository) : IRoleService
{
    public async Task<PagedResult<RoleDto>> SearchRolesAsync(RoleSearchOptions options)
    {
        try
        {
            var result = await roleRepository.SearchRolesAsync(options);
            var dto = new PagedResult<RoleDto>
            {
                Items = result.Select(x => new RoleDto(x)).ToList(),
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

    public async Task<RoleDto> CreateRoleAsync(RoleCreateRequest request)
    {
        try
        {
            var newRole = new Role
            {
                TenantId = request.TenantId,
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            };

            var createdRole = await roleRepository.InsertOrUpdateRoleAsync(newRole);
            await roleRepository.SaveChangesAsync();
            if (createdRole == null)
            {
                throw new Exception("Failed to create role");
            }
            return new RoleDto(createdRole);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(RoleService), "Error creating Role", ex);
            throw;
        }
    }

    public async Task<RoleDto> UpdateRoleAsync(string userId, RoleUpdateRequest user)
    {
        try
        {
            var existingRole = await roleRepository.SearchRolesAsync(new RoleSearchOptions()
            {
                Id = userId,
                Page = 1,
                PageSize = 1
            }).ContinueWith(t => t.Result.FirstOrDefault() ?? null);
            
            if (existingRole == null)
            {
                throw new Exception("Role not found");
            }
            
            existingRole.Name = user.Name ?? existingRole.Name;
            existingRole.Description = user.Description ?? existingRole.Description;
            existingRole.UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var role = await roleRepository.InsertOrUpdateRoleAsync(existingRole);
            await roleRepository.SaveChangesAsync();
            if (role == null)
            {
                throw new Exception("Failed to update role");
            }
            return new RoleDto(role);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(RoleService), "Error updating Role", ex);
            throw;
        }
    }

    public async Task AddPermissionToRoleAsync(string roleName, string permissionName)
    {
        try
        {
            var role = await roleRepository.SearchRolesAsync(new RoleSearchOptions()
            {
                Name = roleName,
                Page = 1,
                PageSize = 1
            }).ContinueWith(t => t.Result.FirstOrDefault() ?? null);
            
            if (role == null)
            {
                throw new Exception($"Role '{roleName}' not found");
            }

            var permission = await permissionRepository.GetPermissionByNameAsync(permissionName);
            if (permission == null)
            {
                throw new Exception($"Permission '{permissionName}' not found");
            }

            if (role.Permissions.All(p => p.Name != permission.Name))
            {
                role.Permissions.Add(permission);
                await roleRepository.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(RoleService), "Error adding Permission to Role",ex);
            throw;
        }
    }

    public async Task DeleteRoleAsync(string id)
    {
        try
        {
            await roleRepository.DeleteRoleAsync(id);
            await roleRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(RoleService), "Error deleting Role", ex);
            throw;
        }
    }
}