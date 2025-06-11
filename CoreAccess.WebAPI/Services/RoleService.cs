using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Repositories;

namespace CoreAccess.WebAPI.Services;

public interface IRoleService
{
    Task<PagedResult<CoreRoleDto>> SearchRolesAsync(CoreRoleSearchOptions options);
    Task<CoreRoleDto> CreateRoleAsync(CoreRoleCreateRequest request);
    Task<CoreRoleDto> UpdateRoleAsync(string userId, CoreRoleUpdateRequest user);
    Task DeleteRoleAsync(string id);
}

public class RoleService(IRoleRepository roleRepository) : IRoleService
{
    public async Task<PagedResult<CoreRoleDto>> SearchRolesAsync(CoreRoleSearchOptions options)
    {
        try
        {
            var result = await roleRepository.SearchRolesAsync(options);
            var dto = new PagedResult<CoreRoleDto>
            {
                Items = result.Select(x => new CoreRoleDto(x)).ToList(),
                TotalCount = result.Count,
                Page = options.Page,
                PageSize = options.PageSize
            };
            return dto;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<CoreRoleDto> CreateRoleAsync(CoreRoleCreateRequest request)
    {
        try
        {
            var newRole = new CoreRole
            {
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
            return new CoreRoleDto(createdRole);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<CoreRoleDto> UpdateRoleAsync(string userId, CoreRoleUpdateRequest user)
    {
        try
        {
            var existingRole = await roleRepository.SearchRolesAsync(new CoreRoleSearchOptions()
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
            return new CoreRoleDto(role);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
            Console.WriteLine(ex);
            throw;
        }
    }
}