using CoreAccess.WebAPI.Model;

namespace CoreAccess.WebAPI.Services;

public interface IUserService
{
    Task<List<CoreUser>> SearchUsersAsync(CoreUserSearchOptions options);
    Task<bool> CreateUserAsync(CoreUserCreateRequest request);
    Task<bool> UpdateUserAsync(string userId, CoreUserUpdateRequest user);
    Task<bool> DeleteUserAsync(string id);
}
internal class UserService : IUserService
{
    public async Task<List<CoreUser>> SearchUsersAsync(CoreUserSearchOptions options)
    {
        throw new NotImplementedException();
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