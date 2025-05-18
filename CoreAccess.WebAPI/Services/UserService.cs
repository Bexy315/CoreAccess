using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Repositories;

namespace CoreAccess.WebAPI.Services;

public interface IUserService
{
    Task<PagedResult<CoreUserDto>> SearchUsersAsync(CoreUserSearchOptions options);
    Task<bool> CreateUserAsync(CoreUserCreateRequest request);
    Task<bool> UpdateUserAsync(string userId, CoreUserUpdateRequest user);
    Task<bool> DeleteUserAsync(string id);
}
internal class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<PagedResult<CoreUserDto>> SearchUsersAsync(CoreUserSearchOptions options)
    {
        try
        {
            var result = await userRepository.SearchUsersAsync(options);
            var dto = new PagedResult<CoreUserDto>
            {
                Items = result.Select(x => new CoreUserDto(x)).ToList(),
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

    public async Task<bool> CreateUserAsync(CoreUserCreateRequest user)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateUserAsync(string userId, CoreUserUpdateRequest user)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        throw new NotImplementedException();
    }
}